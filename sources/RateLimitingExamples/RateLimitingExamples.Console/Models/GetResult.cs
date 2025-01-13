namespace RateLimitingExamples.Console.Models
{
    public class GetResult(string content, DateTimeOffset acquiredAt)
    {
        #region Fields

        private readonly string content = content;
        private readonly DateTimeOffset acquiredAt = acquiredAt;

        #endregion

        #region Properties

        public string Content => this.content;
        public DateTimeOffset AcquiredAt => this.acquiredAt;

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Content: {this.Content}, Acquired At: {this.AcquiredAt}";
        }

        #endregion
    }
}
