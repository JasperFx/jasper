using System;

namespace IntegrationTests.Persistence.Marten.Persistence.Resiliency
{
    [Obsolete("Moved to ST")]
    public class QuestionHandler
    {
        public Answer Handle(Question question)
        {
            return new Answer
            {
                Sum = question.X + question.Y,
                Product = question.X * question.Y
            };
        }
    }

    [Obsolete("Moved to ST")]
    public class Question
    {
        public int X;
        public int Y;
    }

    [Obsolete("Moved to ST")]
    public class Answer
    {
        public int Product;
        public int Sum;
    }
}
