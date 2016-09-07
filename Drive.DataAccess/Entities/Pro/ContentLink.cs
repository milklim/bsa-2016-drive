﻿using System.Collections.Generic;
using Drive.DataAccess.Interfaces;

namespace Drive.DataAccess.Entities.Pro
{
    public class ContentLink : IEntity
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public string Link { get; set; }
        public LinkType LinkType { get; set; }

        public Lecture Lecture { get; set; }
    }

    public enum LinkType
    {
        None,
        Video,
        Slide,
        Sample,
        Useful,
        Repository
    }
}