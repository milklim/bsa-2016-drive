﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Driver.Shared.Dto;
using Drive.DataAccess.Interfaces;
using Drive.Logging;
using Drive.DataAccess.Entities;
using System.Data.Entity;
using Driver.Shared.Dto.Users;
using Drive.Identity.Services;

namespace Drive.WebHost.Services
{
    public class SharedSpaceService : ISharedSpaceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IUsersService _userService;
        private readonly ISpaceService _spaceService;

        public SharedSpaceService(IUnitOfWork unitOfWork, ILogger logger, IUsersService userService, ISpaceService spaceService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userService = userService;
            _spaceService = spaceService;
        }

        public async Task<IEnumerable<UserSharedSpaceDto>> GetPermissionsOfSharedDataAsync(int id)
        {
            try
            {
                var filePermission = await _unitOfWork.SharedSpace.Query
                    .Where(f => f.Content.Id == id)
                    .Select(f => new UserSharedSpaceDto
                    {
                        GlobalId = f.User.GlobalId,
                        IsDeleted = f.IsDeleted,
                        CanModify = f.CanModify,
                        CanRead = f.CanRead
                    }).ToListAsync();
                return filePermission;
            }
            catch (Exception ex)
            {
                _logger.WriteError(ex, ex.Message);
                return null;
            }
        }

        public async Task CreateOrUpdatePermissionsOfSharedDataAsync(List<UserSharedSpaceDto> users, int id)
        {
            try
            {
                foreach (var user in users)
                {
                    var fileShared = await _unitOfWork.SharedSpace.Query.Include(t => t.Content).Include(t => t.User).SingleOrDefaultAsync(f => f.Content.Id == id && f.User.GlobalId == user.GlobalId);

                    if (fileShared == null)
                    {
                        var fileSharedDeleted = await _unitOfWork.SharedSpace.Deleted.Include(t => t.Content).Include(t => t.User).SingleOrDefaultAsync(f => f.Content.Id == id && f.User.GlobalId == user.GlobalId);
                        if (fileSharedDeleted == null)
                        {
                            var userDb = await _unitOfWork.Users.Query.FirstOrDefaultAsync(x => x.GlobalId == user.GlobalId);
                            var file = await _unitOfWork.Files.Query.SingleOrDefaultAsync(f => f.Id == id);
                            if (file != null)
                            {
                                var newSharedFile = new Shared()
                                {
                                    IsDeleted = user.IsDeleted,
                                    Content = file,
                                    User = userDb,
                                    CanModify = user.CanModify,
                                    CanRead = user.CanRead,
                                };
                                _unitOfWork.SharedSpace.Create(newSharedFile);
                            }
                            else
                            {
                                var folder = await _unitOfWork.Folders.Query.SingleOrDefaultAsync(f => f.Id == id);
                                if (folder != null)
                                {
                                    var newSharedFolder = new Shared()
                                    {
                                        IsDeleted = user.IsDeleted,
                                        Content = folder,
                                        User = userDb,
                                        CanModify = user.CanModify,
                                        CanRead = user.CanRead,
                                    };
                                    _unitOfWork.SharedSpace.Create(newSharedFolder);
                                }
                            }
                        }
                        else
                        {
                            fileSharedDeleted.CanModify = user.CanModify;
                            fileSharedDeleted.CanRead = user.CanRead;
                            fileSharedDeleted.IsDeleted = user.IsDeleted;
                            if (!user.CanModify && !user.CanRead)
                                fileShared.IsDeleted = true;
                        }
                    }
                    else
                    {
                        fileShared.CanModify = user.CanModify;
                        fileShared.CanRead = user.CanRead;
                        fileShared.IsDeleted = user.IsDeleted;
                        if (!user.CanModify && !user.CanRead)
                            fileShared.IsDeleted = true;
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.WriteError(ex, ex.Message);
            }
        }

        public async Task<SharedSpaceDto> GetAsync(int page, int count, string sort, int? folderId, int? rootFolderId)
        {
            try
            {
                var sharedContent = await GetSharedContentAsync(folderId, rootFolderId);
                if (sharedContent == null)
                    return null;

                return await GetSharedContentAfterPaginationAsync(sharedContent, page, count, sort);
            }
            catch (Exception ex)
            {
                _logger.WriteError(ex, ex.Message);
                return null;
            }
        }

        public async Task<int> GetTotalAsync(int? folderId, int? rootFolderId)
        {
            try
            {
                var sharedContent = await GetSharedContentAsync(folderId, rootFolderId);
                if (sharedContent == null)
                    return 0;
                int contentCount = 0;
                contentCount += sharedContent.Files.Count();
                contentCount += sharedContent.Folders.Count();

                return contentCount;
            }
            catch (Exception ex)
            {
                _logger.WriteError(ex, ex.Message);
                return 0;
            }
        }

        public async Task<SharedSpaceDto> SearchAsync(string text, int page, int count, string sort, int? folderId, int? rootFolderId)
        {
            try
            {
                var sharedContent = await GetSharedContentAsync(folderId, rootFolderId);
                if (sharedContent == null)
                    return null;

                if (!string.IsNullOrEmpty(text))
                {
                    sharedContent.Files = sharedContent.Files.Where(f => f.Name.ToLower().Contains(text.ToLower()));
                    sharedContent.Folders = sharedContent.Folders.Where(f => f.Name.ToLower().Contains(text.ToLower()));
                }

                return await GetSharedContentAfterPaginationAsync(sharedContent, page, count, sort);
            }
            catch (Exception ex)
            {
                _logger.WriteError(ex, ex.Message);
                return null;
            }
        }

        public async Task<int> SearchTotalAsync(string text, int? folderId, int? rootFolderId)
        {
            try
            {
                var sharedContent = await GetSharedContentAsync(folderId, rootFolderId);
                if (sharedContent == null)
                    return 0;
                int contentCount = 0;
                if (!string.IsNullOrEmpty(text))
                {
                    contentCount += sharedContent.Files.Where(f => f.Name.ToLower().Contains(text.ToLower())).Count();
                    contentCount += sharedContent.Folders.Where(f => f.Name.ToLower().Contains(text.ToLower())).Count();
                }

                return contentCount;
            }
            catch (Exception ex)
            {
                _logger.WriteError(ex, ex.Message);
                return 0;
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                string userId = _userService.CurrentUserId;
                var file = await _unitOfWork.SharedSpace.Query.Include(t => t.Content).Include(t => t.User)
                    .SingleOrDefaultAsync(f => f.Content.Id == id && f.User.GlobalId == userId);
                if (file != null)
                {
                    file.IsDeleted = true;
                }
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.WriteError(ex, ex.Message);
            }
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }

        private async Task<SharedSpaceDto> GetSharedContentAsync(int? folderId, int? rootFolderId)
        {
            string userId = _userService.CurrentUserId;
            IEnumerable<FileUnitDto> files = null;
            IEnumerable<FolderUnitDto> folders = null;
            if (folderId == null && rootFolderId == null)
            {
                files = await _unitOfWork.SharedSpace.Query
                    .Where(s => !s.IsDeleted && !s.Content.IsDeleted && s.User.GlobalId == userId && s.Content is FileUnit)
                    .Select(f => new FileUnitDto()
                    {
                        Description = f.Content.Description,
                        FileType = (f.Content as FileUnit).FileType,
                        Id = f.Content.Id,
                        IsDeleted = f.Content.IsDeleted,
                        Name = f.Content.Name,
                        CreatedAt = f.Content.CreatedAt,
                        Link = (f.Content as FileUnit).Link,
                        Author = new AuthorDto() { Id = f.Content.Owner.Id, GlobalId = f.Content.Owner.GlobalId },
                        CanModify = f.CanModify,
                        CanRead = f.CanRead
                    }).ToListAsync();
                folders = await _unitOfWork.SharedSpace.Query
                    .Where(s => !s.IsDeleted && !s.Content.IsDeleted && s.User.GlobalId == userId && s.Content is FolderUnit)
                    .Select(f => new FolderUnitDto()
                    {
                        Description = f.Content.Description,
                        Id = f.Content.Id,
                        IsDeleted = f.Content.IsDeleted,
                        Name = f.Content.Name,
                        CreatedAt = f.Content.CreatedAt,
                        Author = new AuthorDto() { Id = f.Content.Owner.Id, GlobalId = f.Content.Owner.GlobalId },
                        CanModify = f.CanModify,
                        CanRead = f.CanRead
                    }).ToListAsync();
            }
            else if (folderId == rootFolderId)
            {
                var rootFolder = await _unitOfWork.SharedSpace.Query
                    .Where(s => !s.IsDeleted && s.Content.Id == rootFolderId && !s.Content.IsDeleted && s.User.GlobalId == userId && s.Content is FolderUnit)
                    .Select(f => new {
                        CanRead = f.CanRead,
                        CanModify = f.CanModify
                    }).SingleOrDefaultAsync();
                if (rootFolder != null)
                {
                    folders = await _unitOfWork.Folders.Query
                        .Where(f => f.FolderUnit.Id == folderId)
                        .Select(f => new FolderUnitDto
                        {
                            Description = f.Description,
                            Id = f.Id,
                            IsDeleted = f.IsDeleted,
                            Name = f.Name,
                            CreatedAt = f.CreatedAt,
                            Author = new AuthorDto() { Id = f.Owner.Id, GlobalId = f.Owner.GlobalId },
                            CanModify = rootFolder.CanModify,
                            CanRead = rootFolder.CanRead
                        }).ToListAsync();
                    files = await _unitOfWork.Files.Query
                        .Where(f => f.FolderUnit.Id == folderId)
                        .Select(f => new FileUnitDto()
                        {
                            Description = f.Description,
                            FileType = f.FileType,
                            Id = f.Id,
                            IsDeleted = f.IsDeleted,
                            Name = f.Name,
                            CreatedAt = f.CreatedAt,
                            Link = f.Link,
                            Author = new AuthorDto() { Id = f.Owner.Id, GlobalId = f.Owner.GlobalId },
                            CanModify = rootFolder.CanModify,
                            CanRead = rootFolder.CanRead
                        }).ToListAsync();
                }
            }
            else
            {
                var rootFolder = await _unitOfWork.SharedSpace.Query
                    .Where(s => !s.IsDeleted && s.Content.Id == rootFolderId && !s.Content.IsDeleted && s.User.GlobalId == userId && s.Content is FolderUnit)
                    .Select(f => new {
                        CanRead = f.CanRead,
                        CanModify = f.CanModify
                    }).SingleOrDefaultAsync();
                if (rootFolder != null)
                {
                    if (await CheckExistenceFolderRecursivelyAsync(folderId.Value, rootFolderId.Value))
                    {
                        folders = await _unitOfWork.Folders.Query
                            .Where(f => f.FolderUnit.Id == folderId)
                            .Select(f => new FolderUnitDto
                            {
                                Description = f.Description,
                                Id = f.Id,
                                IsDeleted = f.IsDeleted,
                                Name = f.Name,
                                CreatedAt = f.CreatedAt,
                                Author = new AuthorDto() { Id = f.Owner.Id, GlobalId = f.Owner.GlobalId },
                                CanModify = rootFolder.CanModify,
                                CanRead = rootFolder.CanRead
                            }).ToListAsync();
                        files = await _unitOfWork.Files.Query
                            .Where(f => f.FolderUnit.Id == folderId)
                            .Select(f => new FileUnitDto()
                            {
                                Description = f.Description,
                                FileType = f.FileType,
                                Id = f.Id,
                                IsDeleted = f.IsDeleted,
                                Name = f.Name,
                                CreatedAt = f.CreatedAt,
                                Link = f.Link,
                                Author = new AuthorDto() { Id = f.Owner.Id, GlobalId = f.Owner.GlobalId },
                                CanModify = rootFolder.CanModify,
                                CanRead = rootFolder.CanRead
                            }).ToListAsync();
                    }
                    else
                        return null;
                }
            }
            return new SharedSpaceDto() { Files = files, Folders = folders };
        }

        private async Task<SharedSpaceDto> GetSharedContentAfterPaginationAsync(SharedSpaceDto sharedContent, int page, int count, string sort)
        {
            if (sort != null && sort.Equals("asc"))
            {
                var resultFolders = sharedContent.Folders.OrderBy(f => f.CreatedAt);
                var resultFiles = sharedContent.Files.OrderBy(f => f.CreatedAt);

                sharedContent.Folders = resultFolders;
                sharedContent.Files = resultFiles;
            }
            else if (sort != null && sort.Equals("desc"))
            {
                var resultFolders = sharedContent.Folders.OrderByDescending(f => f.CreatedAt);
                var resultFiles = sharedContent.Files.OrderByDescending(f => f.CreatedAt);

                sharedContent.Folders = resultFolders;
                sharedContent.Files = resultFiles;
            }

            int skipCount = (page - 1) * count;
            if (sharedContent.Folders.Count() <= skipCount)
            {
                skipCount -= sharedContent.Folders.Count();
                sharedContent.Folders = new List<FolderUnitDto>();
                sharedContent.Files = sharedContent.Files.Skip(skipCount).Take(count);
            }
            else
            {
                sharedContent.Folders = sharedContent.Folders.Skip(skipCount).Take(count);
                count -= sharedContent.Folders.Count();
                sharedContent.Files = sharedContent.Files.Take(count);
            }

            var owners = (await _userService.GetAllAsync()).Select(f => new { Id = f.id, Name = f.name });
            Parallel.ForEach(sharedContent.Files, file =>
            {
                file.Author.Name = owners.FirstOrDefault(o => o.Id == file.Author.GlobalId)?.Name;
            });
            Parallel.ForEach(sharedContent.Folders, folder =>
            {
                folder.Author.Name = owners.FirstOrDefault(o => o.Id == folder.Author.GlobalId)?.Name;
            });
            if (sharedContent.Files.Count() == 0 && sharedContent.Folders.Count() == 0)
                return null;
            else
                return sharedContent;
        }

        private async Task<bool> CheckExistenceFolderRecursivelyAsync(int folderId, int rootFolderId)
        {
            var foldersId = await _unitOfWork.Folders.Query
                .Where(f => f.FolderUnit.Id == rootFolderId)
                .Select(f => new { Id = f.Id })
                .ToListAsync();
            foreach (var folder in foldersId)
                if (folder != null && folder.Id == folderId)
                    return true;
            var folders = await _unitOfWork.Folders.Query
                .Where(f => f.FolderUnit.Id == rootFolderId)
                .Select(f => new { ContentList = f.DataUnits })
                .ToListAsync();
            for (int i = 0; i < folders.Count; i++)
            {
                for (int j = 0; j < folders[i].ContentList.Count; j++)
                {
                    if (folders[i].ContentList[j] is FolderUnit)
                    {
                        if (folders[i].ContentList[j].Id == folderId)
                            return true;
                        else
                        {
                            if (await CheckExistenceFolderRecursivelyAsync(folderId, folders[i].ContentList[j].Id))
                                return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}