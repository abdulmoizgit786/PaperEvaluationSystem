using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Evalin.Models
{
    public class Dashboard
    {
        public User user { get; set; }
        public List<Course> Courses { get; set; }

    }
}