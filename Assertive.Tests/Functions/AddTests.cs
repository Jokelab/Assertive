using Assertive.Functions;
using Assertive.Types;

namespace Assertive.Tests.Functions
{
    public class AddTests
    {
        [Fact]
        public async Task AddShouldAddNewEntry()
        {
            var sut = new Add();
            var dic = new DictionaryValue();
            var context = new FunctionContext() { FilePath = "c:\\unittest.ass" };
            var values = new List<Value>
            {
                dic,
                new StringValue("key"),
                new StringValue("value")
            };

            await sut.Execute(values, context);

            Assert.NotEmpty(dic.GetEntries());
        } 
    }
}
