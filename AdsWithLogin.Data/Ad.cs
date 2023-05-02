using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdsWithLogin.Data
{
    public class Ad
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }
        public bool Delete { get; set; }
    }
}
