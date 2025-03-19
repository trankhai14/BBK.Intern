using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;

namespace MyProject.Sliders
{
	[Table("AppSliders")]
	public class Slider : Entity, IHasCreationTime
	{
		[StringLength(255)]
		public string Title { get; set; }
		public string Description { get; set; }
		public string Image { get; set; }
		public DateTime CreationTime { get; set; }
		public bool IsActive { get; set; }

		

		public Slider()
		{
			CreationTime = Clock.Now;
		}

		public Slider(string title, string description=null, bool isActive = false)
			: this()
		{
			Title = title;
			Description = description;
			IsActive = isActive;
		}
	}
}
