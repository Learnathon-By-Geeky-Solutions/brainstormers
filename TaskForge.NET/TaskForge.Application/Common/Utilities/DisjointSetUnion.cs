namespace TaskForge.Application.Common.Utilities
{
    public class DisjointSetUnion
    {
        private readonly Dictionary<int, int> parent = new();
        private readonly Dictionary<int, int> rank = new();

        public void MakeSet(int x)
        {
            if (!parent.ContainsKey(x))
            {
                parent[x] = x;
                rank[x] = 0;
            }
        }

        public int Find(int x)
        {
            if (parent[x] != x)
                parent[x] = Find(parent[x]);
            return parent[x];
        }

        public void Union(int x, int y)
        {
            int rootX = Find(x);
            int rootY = Find(y);

            if (rootX == rootY) return;

            if (rank[rootX] < rank[rootY])
                parent[rootX] = rootY;
            else if (rank[rootX] > rank[rootY])
                parent[rootY] = rootX;
            else
            {
                parent[rootY] = rootX;
                rank[rootX]++;
            }
        }
    }
}
