namespace Jasper.Marten.Tests.Persistence.Resiliency
{
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

    public class Question
    {
        public int X;
        public int Y;
    }

    public class Answer
    {
        public int Sum;
        public int Product;
    }
}
