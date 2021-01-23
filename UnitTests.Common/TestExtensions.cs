using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnitTests.Common
{
    public static class TestExtensions
    {
        // From a comment on https://gist.github.com/DCCoder90/d358ace7ef36401dd6f0449d4ab87706
        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
            {
                yield return await Task.FromResult(item);
            }
        }
    }
}
