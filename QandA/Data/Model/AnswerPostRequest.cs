using System;
using System.ComponentModel.DataAnnotations;

namespace QandA.Data
{
    public class AnswerPostRequest
    {
        [Required]
        public int? QuestionId { get; set; }
        [Required]
        public string Content { get; set; }
  
    }
}