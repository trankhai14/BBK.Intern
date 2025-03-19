using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities.Auditing;

namespace MyProject.People
{
    [Table("AppPerson")]
    public class Person : AuditedEntity<int>
    {
        public const int MaxNameLength = 32;

        [Required]
        [StringLength(MaxNameLength)]
        public string Name { get; set; } 

        public Person() 
        {

        }

        public Person(string name)
        {
            Name = name;
        }
    }
}
