using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QandA.Data;
using QandA.Data.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QandA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IDataRepository _dataRepository;
        public QuestionsController(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }


        [HttpGet]
        public IEnumerable<QuestionGetManyResponse> GetQuestions(string search,
    bool includeAnswers,
    int page = 1,
    int pageSize = 20
  )
        { 
            if (string.IsNullOrWhiteSpace(search))
            {
                if (includeAnswers)
                {
                    return _dataRepository.GetQuestionsWithAnswers();
                }
                else
                {

                    return _dataRepository.GetQuestions();
                }
            }
            else
            {

                return _dataRepository.GetQuestionBySearchWithPaging(search, page, pageSize);
            }

         
        }


        [HttpGet("unanswered")]
        public IEnumerable<QuestionGetManyResponse> GetUnansweredQuestions()
        {
            return _dataRepository.GetUnansweredQuestions();
        }

        [HttpGet("questionId")]
        public ActionResult<QuestionGetSingleResponse> GetQuestion(int questionId)
        {
            var question = _dataRepository.GetQuestion(questionId);
            if (question == null)
            {
                return NotFound();
            }

            return question;
        }


        [HttpPost]
        public ActionResult<QuestionGetSingleResponse> PostQuestion(QuestionPostRequest questionPostRequest)
        {
            var saveQuestion = _dataRepository.PostQuestion(new QuestionPostFullRequest
            {
                Title = questionPostRequest.Title,
                Content = questionPostRequest.Content,
                UserId = "1",
                UserName = "bob.test@test.com",
                Created = DateTime.UtcNow
            });

           return CreatedAtAction(nameof(GetQuestion), new { questionId = saveQuestion.QuestionId }, saveQuestion);

        }

        [HttpPut("{questionId}")]
        public ActionResult<QuestionGetSingleResponse> PutQuestion(int questionId, QuestionPutRequest questionPutRequest)
        {
            var question = _dataRepository.GetQuestion(questionId);
            if(question == null)
            {
                return NotFound();
            }

            questionPutRequest.Title = string.IsNullOrEmpty(questionPutRequest.Title) ? question.Title : questionPutRequest.Title;

            question.Content = string.IsNullOrEmpty(questionPutRequest.Content) ? question.Content : questionPutRequest.Content;

            var savedQustion = _dataRepository.PutQuestion(questionId, questionPutRequest);

            return savedQustion;


        }


        [HttpDelete("{questionId}")]
        public ActionResult DeleteQuestion(int questionId)
        {
            var question = _dataRepository.GetQuestion(questionId);
            if(question == null)
            {
                return NotFound();
            }
            _dataRepository.DeleteQuestion(questionId);

            return NoContent();
        }

        [HttpPost("answer")]
        public ActionResult<AnswerGetResponse> PostAnswer(AnswerPostRequest answerPostRequest)
        {
            var questionExists = _dataRepository.QuestionExists(answerPostRequest.QuestionId.Value);
            if(!questionExists)
            {
                return NotFound();
            }
            var savedAnswer =
               _dataRepository.PostAnswer(new AnswerPostFullRequest
               {
                   QuestionId = answerPostRequest.QuestionId.Value,
                   Content = answerPostRequest.Content,
                   UserId = "1",
                   UserName = "bob.test@test.com",
                   Created = DateTime.UtcNow
               }
               );
            return savedAnswer;
        }
    }

}
