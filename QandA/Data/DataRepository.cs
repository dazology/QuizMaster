﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using QandA.Data.Model;
using static Dapper.SqlMapper;

namespace QandA.Data
{
    public class DataRepository : IDataRepository
    {
        private readonly string _connectionString;

        public DataRepository(IConfiguration configuration)
        {

            _connectionString = configuration["ConnectionString:DefaultConnection"];
        }

        public void DeleteQuestion(int questionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                connection.Execute(
                  @"EXEC dbo.Question_Delete 
        @QuestionId = @QuestionId",
                  new { QuestionId = questionId }
                );
            }
        }

        public AnswerGetResponse GetAnswer(int answerId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.QueryFirstOrDefault<AnswerGetResponse>(
                  @"EXEC dbo.Answer_Get_ByAnswerId @AnswerId = @AnswerId", new { AnswerId = answerId });
            };
        }

        public IEnumerable<QuestionGetManyResponse> GetQuestion()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                return connection.Query<QuestionGetManyResponse>(
                    @"EXEC dbo.Question_GetMany");
            }
        }

        public QuestionGetSingleResponse GetQuestion(int questionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (GridReader results = connection.QueryMultiple(
                  @"EXEC dbo.Question_GetSingle 
                      @QuestionId = @QuestionId; 
                    EXEC dbo.Answer_Get_ByQuestionId 
                      @QuestionId = @QuestionId",
                     new { QuestionId = questionId }
            )
            )
                {
                    var question = results.Read<QuestionGetSingleResponse>().FirstOrDefault();

                    if (question != null)
                    {
                        question.Answers = results.Read<AnswerGetResponse>().ToList();
                    }


                    return question;
                }
            }
        }

        public IEnumerable<QuestionGetManyResponse> GetQuestionBySearchWithPaging(string search, int pageNumber, int pageSize)
        {
            using(var connetion = new SqlConnection(_connectionString))
            {
                connetion.Open();

                var parameters = new
                {
                    Search = search,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return connetion.Query<QuestionGetManyResponse>(
                   " @EXEC dbo.Qjuestion_GetMany_BySearch_WithPaging @Search = @Search, @PageNumber = @PageNumber", parameters);
            }
        }

        public IEnumerable<QuestionGetManyResponse> GetQuestions()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var question = connection.Query<QuestionGetManyResponse>(
                    @"EXEC dbo.Question_GetMany");

                return question;
            }

        }

        public IEnumerable<QuestionGetManyResponse> GetQuestionsBySearch(string search)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<QuestionGetManyResponse>(
                  @"EXEC dbo.Question_GetMany_BySearch @Search = @Search",
                  new { Search = search }
                );
            }
        }

        public IEnumerable<QuestionGetManyResponse> GetQuestionsWithAnswers()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();


                var questionDictionary =
                  new Dictionary<int, QuestionGetManyResponse>();

                return connection
                    .Query<
                        QuestionGetManyResponse,
                        AnswerGetResponse,
                        QuestionGetManyResponse>(
                            "EXEC dbo.Question_GetMany_WithAnswers",
                    map: (q, a) =>
                    {
                        QuestionGetManyResponse question;

                        if (!questionDictionary.TryGetValue(q.QuestionId, out question))
                        {
                            question = q;
                            question.Answers = new List<AnswerGetResponse>();
                        }
                        question.Answers.Add(a);
                        return question;
                    },
                        splitOn: "QuestionId"
                        ).Distinct().ToList();
            }
        }



        public IEnumerable<QuestionGetManyResponse> GetUnansweredQuestions()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<QuestionGetManyResponse>(
                    "EXEC dboQuestion_GetUnanswered");
            }
        }

        public AnswerGetResponse PostAnswer(AnswerPostFullRequest answer)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.QueryFirst<AnswerGetResponse>(
                  @"EXEC dbo.Answer_Post 
        @QuestionId = @QuestionId, @Content = @Content, 
        @UserId = @UserId, @UserName = @UserName,
        @Created = @Created",
                  answer
                );
            }
        }
        public QuestionGetSingleResponse PostQuestion(QuestionPostFullRequest question)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var questionId = connection.QueryFirst<int>(
                    @"EXEC dbo.Question_Post
                        @Title = @Title, @Content = @Content,
                        @UserId = @UserId, @UserName = @UserName,
                        @Created = @Created", question
                    );

                return GetQuestion(questionId);
            }
        }

        public QuestionGetSingleResponse PutQuestion(int questionId, QuestionPutRequest question)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                connection.Execute(
                  @"EXEC dbo.Question_Put 
        @QuestionId = @QuestionId, @Title = @Title, @Content = @Content",
                  new { QuestionId = questionId, question.Title, question.Content }
                );
                return GetQuestion(questionId);
            }
        }

        public bool QuestionExists(int questionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.QueryFirst<bool>(
                    @"EXEC dbo.Question_Exists @QuestionId = @QuestionId", new { QuestionId = questionId });
            }
        }
    }
}
