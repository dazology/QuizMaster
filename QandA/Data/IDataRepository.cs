
using System.Collections;
using System.Collections.Generic;
using QandA.Data.Model;

namespace QandA.Data
{
    public interface IDataRepository
    {
        IEnumerable<QuestionGetManyResponse> GetQuestion();

        IEnumerable<QuestionGetManyResponse> GetQuestions();

        IEnumerable<QuestionGetManyResponse>
            GetQuestionsBySearch(string search);

        IEnumerable<QuestionGetManyResponse> GetUnansweredQuestions();

        QuestionGetSingleResponse GetQuestion(int questionId);

        bool QuestionExists(int questionId);

        AnswerGetResponse GetAnswer(int answerId);

        QuestionGetSingleResponse PostQuestion(QuestionPostFullRequest question);

        QuestionGetSingleResponse
          PutQuestion(int questionId, QuestionPutRequest question);

        void DeleteQuestion(int questionId);

        AnswerGetResponse PostAnswer(AnswerPostFullRequest answer);

        IEnumerable<QuestionGetManyResponse> GetQuestionsWithAnswers();

        IEnumerable<QuestionGetManyResponse> GetQuestionBySearchWithPaging(string search, int pageNumber, int pageSize);


    }
}
