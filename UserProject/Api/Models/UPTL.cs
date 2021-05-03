using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models
{
    public class UPTL
    {

        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserSurName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public DateTime DH { get; set; }


    }

    public class Page
    {
        public int PageNumber { get; set; }
        public int PageRows { get; set; }
        public bool CheckCount { get; set; }
       
    }

}
