using System;
using SQLite;

namespace Visage.Models
{
    public class Profile
    {
        [PrimaryKey, AutoIncrement]
        public int Id
        {
            get;
            set;
        }

        public string Thumbnail
        {
            get;
            set;
        }

        public string FullName
        {
            get;
            set;
        }

        public string Email
        {
            get;
            set;
        }

        public double Rating
        {
            get;
            set;
        }
    }
}
