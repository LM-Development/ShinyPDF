namespace OpenQuestPDF.Infrastructure
{
    internal interface IContentDirectionAware
    {
        public ContentDirection ContentDirection { get; set; }
    }
}