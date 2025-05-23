﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;
using MyProject.People;

namespace MyProject.Tasks
{
    [Table("AppTasks")]
    public class Task : Entity, IHasCreationTime 
    {
        public const int MaxTitleLength = 256;
        public const int MaxDescriptionLength = 64 * 1024; //64KB
        public string AssignedPersonName;

        [Required]
        [StringLength(MaxTitleLength)]
        public string Title { get; set; }

        [StringLength(MaxDescriptionLength)]
        public string Description { get; set; }

        public DateTime CreationTime { get; set; }

        public TaskState State { get; set; }

        public Task()
        {
            CreationTime = Clock.Now;
            State = TaskState.Open;
        }

        public Task(string title, string description = null)
            : this()
        {
            Title = title;
            Description = description;
        }

        public enum TaskState : byte
        {
            Open = 0,
            Completed = 1
        }

        [ForeignKey(nameof(AssignedPersonId))]
        public Person AssignedPerson { get; set; }
        public int AssignedPersonId { get; set; }

        public Task(string title, string description = null, int assignedPersonId =0)
            : this()
        {
            Title = title;
            Description = description;
            AssignedPersonId = assignedPersonId;
        }
    }
}
