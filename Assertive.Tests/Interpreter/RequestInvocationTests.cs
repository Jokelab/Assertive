using Assertive.Requests;
using Assertive.Requests.Http;
using Moq;

namespace Assertive.Tests.Interpreter
{
    public class RequestInvocationTests : InterpreterTests
    {
        [Fact]
        public async Task SimpleRequest()
        {
            const string program = "POST 'http://www.testuri.com';";

            await Sut.Execute(program);

            Assert.Single(TestRecorder.Requests);
        }

        [Fact]
        public async Task MultipleRequests()
        {
            const string program = @"POST 'http://www.testuri.com'; 
                                     GET 'http://www.testuri.com';
                                     PUT 'http://www.testuri.com';
                                     DELETE 'http://www.testuri.com';";

            await Sut.Execute(program);

            Assert.Equal(4, TestRecorder.Requests.Count);
        }

        [Theory]
        [InlineData("POST")]
        [InlineData("GET")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("PATCH")]
        [InlineData("OPTIONS")]
        [InlineData("HEAD")]
        [InlineData("TRACE")]
        [InlineData("CONNECT")]
        public async Task StaticHttpMethod(string method)
        {
            var program = $"{method} 'http://www.testuri.com';";

            await Sut.Execute(program);

            Assert.Single(TestRecorder.Requests);
            Assert.Equal(method, ((HttpRequest)TestRecorder.Requests[0]).Request.Method.Method);
        }

        [Fact]
        public async Task DynamicHttpMethod()
        {
            const string program = @"
                                $method = 'PO';
                                '{{$method}}ST' 'http://www.testuri.com';";

            await Sut.Execute(program);

            Assert.Single(TestRecorder.Requests);
            Assert.Equal(HttpMethod.Post, ((HttpRequest)TestRecorder.Requests[0]).Request.Method);
        }

        [Fact]
        public async Task SetHttpHeadersInline()
        {
            const string program = @"
                                    POST 'http://www.testuri.com' 
                                        headers { 'x-custom-header': 3 * 5 };";

            await Sut.Execute(program);

            Assert.Single(TestRecorder.Requests);
            var header = ((HttpRequest)TestRecorder.Requests[0]).Request.Headers.GetValues("x-custom-header");
            Assert.Equal("15", header.First());
        }

        [Fact]
        public async Task SetHttpHeadersWithVariable()
        {
            const string program = @"
                                    $x = 3;
                                    $custom = {'x-custom-header' : $x * 9};
                                    GET 'http://www.testuri.com' headers $custom;";

            await Sut.Execute(program);

            Assert.Single(TestRecorder.Requests);
            var header = ((HttpRequest)TestRecorder.Requests[0]).Request.Headers.GetValues("x-custom-header");
            Assert.Equal("27", header.First());
        }

        [Fact]
        public async Task SetQueryInline()
        {
            const string program = @"
                                    POST 'http://www.testuri.com' 
                                        query { 'id': 3 * 5 };";

            await Sut.Execute(program);

            Assert.Single(TestRecorder.Requests);
            Assert.Equal("?id=15", ((HttpRequest)TestRecorder.Requests[0]).Request!.RequestUri!.Query);
        }

        [Fact]
        public async Task SetQueryWithVariable()
        {
            const string program = @"
                                    $x = 3;
                                    $custom = {'id' : $x * 9, 'name':'DaddyCool'};
                                    GET 'http://www.testuri.com' query $custom;";

            await Sut.Execute(program);

            Assert.Single(TestRecorder.Requests);
            Assert.Equal("?id=27&name=DaddyCool", ((HttpRequest)TestRecorder.Requests[0]).Request!.RequestUri!.Query);
        }

        [Fact]
        public async Task BodyShouldSupportImplicitStreamContent()
        {
            //stream keyword omitted
            const string program = @"POST 'http://www.testuri.com' body FileToStream('somefile.test');";

            await Sut.Execute(program);

            Assert.Single(TestRecorder.Requests);
            Assert.True( ((HttpRequest)TestRecorder.Requests[0]).Request!.Content is StreamContent);
        }

        [Fact]
        public async Task BodyShouldSupportExplicitStreamContent()
        {
            //uses the stream keyword explicitly
            const string program = @"POST 'http://www.testuri.com' body stream FileToStream('somefile.test');";

            await Sut.Execute(program);

            Assert.Single(TestRecorder.Requests);
            Assert.True(((HttpRequest)TestRecorder.Requests[0]).Request!.Content is StreamContent);
        }

        [Fact]
        public async Task BodyShouldSupportImplicitStringContent()
        {
            const string program = @"POST 'http://www.testuri.com' body 'this is the content';";

            await Sut.Execute(program);

            Assert.Single(TestRecorder.Requests);
            Assert.True(((HttpRequest)TestRecorder.Requests[0]).Request!.Content is StringContent);
        }

        [Fact]
        public async Task BodyShouldSupportExplicitStringContent()
        {
            const string program = @"POST 'http://www.testuri.com' body string 'this is the content';";

            await Sut.Execute(program);

            Assert.Single(TestRecorder.Requests);
            Assert.True(((HttpRequest)TestRecorder.Requests[0]).Request!.Content is StringContent);
        }

        [Fact]
        public async Task BodyShouldSupportFormUrlEncodedContent()
        {
            const string program = @"POST 'http://www.testuri.com' body formurlencoded 
                                    {'Firstname': 'John', 'Lastname': 'Doe'};";

            await Sut.Execute(program);

            Assert.Single(TestRecorder.Requests);
            Assert.True(((HttpRequest)TestRecorder.Requests[0]).Request!.Content is FormUrlEncodedContent);
        }

        [Fact]
        public async Task BodyShouldSupportMultipartFormDataContent()
        {
            const string program = @"POST 'http://www.testuri.com' body formdata 
                                    {'Firstname': 'John', 'Lastname': 'Doe'};";

            await Sut.Execute(program);

            Assert.Single(TestRecorder.Requests);
            Assert.True(((HttpRequest)TestRecorder.Requests[0]).Request!.Content is MultipartFormDataContent);
        }

        [Fact]
        public async Task RequestWithAllOptions()
        {
            const string program = @"POST 'http://www.testuri.com' 
                                    query {'id' : 1, 'name':'John'}
                                    headers {'my-header': 'header-value'}
                                    body formdata 
                                    {'Firstname': 'John', 'Lastname': 'Doe'};";

            await Sut.Execute(program);

            Assert.Single(TestRecorder.Requests);
            var request = ((HttpRequest)TestRecorder.Requests[0]).Request;
            Assert.True(request.Content is MultipartFormDataContent);
            Assert.Equal("http://www.testuri.com/?id=1&name=John", request.RequestUri!.ToString());
            var header = request.Headers.GetValues("my-header");
            Assert.Equal("header-value", header.First());
        }
    }
}
