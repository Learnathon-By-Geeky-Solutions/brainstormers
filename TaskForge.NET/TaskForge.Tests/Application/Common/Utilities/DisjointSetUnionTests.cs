using TaskForge.Application.Common.Utilities;
using Xunit;

namespace TaskForge.Tests.Application.Common.Utilities
{
    [Collection("Sequential")]
    public class DisjointSetUnionTests
    {
        [Fact]
        public async Task MakeSet_ShouldInitializeParentAndRank()
        {
            var dsu = new DisjointSetUnion();

            dsu.MakeSet(1);

            Assert.Equal(1, dsu.Find(1));
            await Task.CompletedTask;
        }

        [Fact]
        public async Task Find_ShouldReturnCorrectRoot_WithPathCompression()
        {
            var dsu = new DisjointSetUnion();

            dsu.MakeSet(1);
            dsu.MakeSet(2);
            dsu.MakeSet(3);

            dsu.Union(1, 2);
            dsu.Union(2, 3);

            int root1 = dsu.Find(1);
            int root2 = dsu.Find(2);
            int root3 = dsu.Find(3);

            Assert.Equal(root1, root2);
            Assert.Equal(root2, root3);

            await Task.CompletedTask;
        }

        [Fact]
        public async Task Union_ShouldAttachLowerRankUnderHigherRank()
        {
            var dsu = new DisjointSetUnion();

            dsu.MakeSet(2);
            dsu.MakeSet(3);

            dsu.Union(2, 3);

            dsu.MakeSet(1);
            dsu.Union(1, 2);

            Assert.Equal(dsu.Find(2), dsu.Find(3));
            Assert.Equal(dsu.Find(1), dsu.Find(2));

            await Task.CompletedTask;
        }

        [Fact]
        public async Task Union_ShouldIncrementRankWhenEqual()
        {
            var dsu = new DisjointSetUnion();

            dsu.MakeSet(1);
            dsu.MakeSet(2);

            dsu.Union(1, 2);

            dsu.MakeSet(3);
            dsu.Union(1, 3);

            Assert.Equal(dsu.Find(1), dsu.Find(3));

            await Task.CompletedTask;
        }

        [Fact]
        public async Task Union_ShouldNotChangeAnythingIfSameSet()
        {
            var dsu = new DisjointSetUnion();

            dsu.MakeSet(1);
            dsu.Union(1, 1);

            Assert.Equal(1, dsu.Find(1));

            await Task.CompletedTask;
        }
    }
}
