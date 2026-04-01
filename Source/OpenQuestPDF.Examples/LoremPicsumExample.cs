using System.Net.Http;
using NUnit.Framework;
using OpenQuestPDF.Examples.Engine;
using OpenQuestPDF.Fluent;
using OpenQuestPDF.Infrastructure;

namespace OpenQuestPDF.Examples
{
    public class LoremPicsum : IComponent
    {
        public bool Greyscale { get; }

        public LoremPicsum(bool greyscale)
        {
            Greyscale = greyscale;
        }

        public void Compose(IContainer container)
        {
            var url = "https://picsum.photos/300/200";

            if (Greyscale)
                url += "?grayscale";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("user-agent", "OpenQuestPDF/1.0 Unit Testing");

            var response = client.GetByteArrayAsync(url);
            response.Wait();
            container.Image(response.Result);
        }
    }

    public class LoremPicsumExample
    {
        [Test]
        public void LoremPicsum()
        {
            RenderingTest
                .Create()
                .PageSize(350, 280)
                .ProducePdf()
                .ShowResults()
                .Render(container =>
                {
                    container
                        .Background("#FFF")
                        .Padding(25)
                        .Column(column =>
                        {
                            column.Spacing(10);

                            column
                                .Item()
                                .Component(new LoremPicsum(true));

                            column
                                .Item()
                                .AlignRight()
                                .Text("From Lorem Picsum");
                        });
                });
        }
    }
}
