using FluentAssertions;
using TaskForge.Application.Common.Utilities;
using Xunit;

namespace TaskForge.Tests.Application.Common.Utilities
{
    [Collection("Sequential")]
    public class DisjointSetUnionTests
    {
        [Fact]
        public void MakeSet_ShouldInitializeParentAndRank()
        {
            var dsu = new DisjointSetUnion();

            dsu.MakeSet(1);

            dsu.Find(1).Should().Be(1);
        }


        [Fact]
        public void Find_ShouldReturnCorrectRoot_WithPathCompression()
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

            root1.Should().Be(root2).And.Be(root3);
        }


        [Fact]
        public void Union_ShouldAttachLowerRankUnderHigherRank()
        {
            var dsu = new DisjointSetUnion();

            dsu.MakeSet(2);
            dsu.MakeSet(3);

            dsu.Union(2, 3);

            dsu.MakeSet(1);
            dsu.Union(1, 2);

            dsu.Find(2).Should().Be(dsu.Find(3));
            dsu.Find(1).Should().Be(dsu.Find(2));
        }
        [Fact]
        public void Union_ShouldIncrementRankWhenEqual()
        {
            var dsu = new DisjointSetUnion();

            dsu.MakeSet(1);
            dsu.MakeSet(2);

            dsu.Union(1, 2);

            dsu.MakeSet(3);
            dsu.Union(1, 3);

            dsu.Find(1).Should().Be(dsu.Find(3));
        }
        [Fact]
        public void Union_ShouldNotChangeAnythingIfSameSet()
        {
            var dsu = new DisjointSetUnion();

            dsu.MakeSet(1);
            dsu.Union(1, 1);

            dsu.Find(1).Should().Be(1);
        }
    }
}
