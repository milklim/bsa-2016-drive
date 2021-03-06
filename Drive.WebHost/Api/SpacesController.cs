﻿using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Drive.DataAccess.Entities;
using Drive.WebHost.Services;
using Driver.Shared.Dto;
using System.Collections.Generic;

namespace Drive.WebHost.Api
{
    [RoutePrefix("api/spaces")]
    public class SpacesController : ApiController
    {

        private readonly ISpaceService _spaceService;

        public SpacesController(ISpaceService spaceService)
        {
            _spaceService = spaceService;
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetAllAsync()
        {
            var result = await _spaceService?.GetAllAsync();

            if (result == null || result.Count == 0)
                return NotFound();

            return Ok(result);
        }

        // GET: api/spaces/(int)/?id=(int)&page=(int)&count=(int)&sort=(string)
        [HttpGet]
        public async Task<IHttpActionResult> GetSpace(int id, int page = 1, int count = 100, string sort = null)
        {
            var result = await _spaceService?.GetAsync(id, page, count, sort);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // GET: api/spaces/{spaceType=(string)}
        [HttpGet]
        [Route("spacetype/{spaceType}")]
        public async Task<IHttpActionResult> GetSpaceByType(string spaceType)
        {
            SpaceType sType;
            if (!System.Enum.TryParse(spaceType, true, out sType))
                return NotFound();
            var result = await _spaceService?.GetSpaceByTypeAsync(sType);
            if (result == 0)
                return NotFound();
            return Ok(result);
        }

        // GET: api/spaces/(int)/sptotal
        [HttpGet]
        [Route("{id:int}/sptotal")]
        public async Task<IHttpActionResult> GetSpaceTotal(int id)
        {
            var result = await _spaceService?.GetTotalAsync(id);

            if (result == 0)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IHttpActionResult> CreateSpace(SpaceDto space)
        {
            int id = await _spaceService?.CreateAsync(space);
            return Ok(id);
        }

        // Delete Space with staff
        [HttpDelete]
        [Route("{id:int}/staff")]
        public async Task<IHttpActionResult> DeleteSpaceWithStaff(int id)
        {
            var result = await _spaceService?.GetAsync(id);

            if (result == null)
                return NotFound();

            await _spaceService?.DeleteWithStaff(id);
            return Ok();
        }

        [HttpDelete]
        public async Task<IHttpActionResult> DeleteSpace(int id)
        {
            var result = await _spaceService?.GetAsync(id);

            if (result == null)
                return NotFound();

            await _spaceService?.Delete(id);
            return Ok();
        }

        [HttpPut]
        public async Task<IHttpActionResult> UpdateSpace(int id, SpaceDto space)
        {
            await _spaceService?.UpdateAsync(id, space);
            return Ok();
        }


        // GET: api/spaces/(int)/search?folderId=(int?)&text=(string)&page=(int)&count=(int)
        [HttpGet]
        [Route("{spaceId:int}/search")]
        public async Task<IHttpActionResult> SearchFolderAndFile(int spaceId, string text = "", int page = 1, int count = 100, int? folderId = null)
        {
            text = text == null ? string.Empty : text;
            var searchResultDto = await _spaceService?.SearchFoldersAndFilesAsync(spaceId, folderId, text, page, count);

            if (searchResultDto == null || (searchResultDto.Files.Count == 0 && searchResultDto.Folders.Count == 0))
                return NotFound();
            return Ok(searchResultDto);
        }

        // GET: api/spaces/(int)/total?folderId=(int?)&text=(string)
        [HttpGet]
        [Route("{spaceId:int}/total")]
        public async Task<IHttpActionResult> NumberOfFoundFoldersAndFiles(int spaceId, string text = "", int? folderId = null)
        {
            text = text == null ? string.Empty : text;
            int result = await _spaceService?.NumberOfFoundFoldersAndFilesAsync(spaceId, folderId, text);
            if (result == 0)
                return NotFound();
            return Ok(result);
        }

        // PUT: api/spaces/movecontent
        [HttpPut]
        [Route("movecontent")]
        public async Task<IHttpActionResult> MoveContent(CopyMoveContentDto content)
        {
            await _spaceService.MoveContentAsync(content);
            return Ok();
        }

        // PUT: api/spaces/copycontent
        [HttpPut]
        [Route("copycontent")]
        public async Task<IHttpActionResult> CopyContent(CopyMoveContentDto content)
        {
            await _spaceService.CopyContentAsync(content);
            return Ok();
        }

        // DELETE: api/spaces/deletecontent
        [HttpPut]
        [Route("deletecontent")]
        public async Task<IHttpActionResult> DeleteContent(CopyMoveContentDto content)
        {
            await _spaceService.DeleteContentAsync(content);
            return Ok();
        }
    }
}