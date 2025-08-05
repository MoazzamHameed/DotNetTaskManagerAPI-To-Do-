using System;


namespace TaskManagerAPI.DTOs

{
    public class TaskDto
    {
        public int? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; } // Pending, Completed, InProgress
        public DateTime DueDate { get; set; }

 
    }
}

