using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace QuizApp.Code
{
    public static class CategoryService
    {
        /// <summary>Active top-level categories with their active sub-categories.</summary>
        public static List<CategoryItem> GetTree()
        {
            var all = Db.List(
                @"SELECT c.Id, c.Name, c.ParentId, c.Description,
                         (SELECT COUNT(*) FROM Quizzes q WHERE q.CategoryId = c.Id AND q.IsActive = 1) AS QuizCount
                  FROM Categories c
                  WHERE c.IsActive = 1
                  ORDER BY c.Name",
                r => new CategoryItem
                {
                    Id = r.Int("Id"),
                    Name = r.Str("Name"),
                    ParentId = r.NInt("ParentId"),
                    Description = r.NStr("Description"),
                    QuizCount = r.Int("QuizCount")
                });

            var roots = all.Where(c => c.ParentId == null).ToList();
            foreach (var root in roots)
                root.SubCategories = all.Where(c => c.ParentId == root.Id).ToList();

            // Sub-categories whose parent is inactive are shown at the top level.
            var rootIds = new HashSet<int>(roots.Select(r => r.Id));
            roots.AddRange(all.Where(c => c.ParentId != null && !rootIds.Contains(c.ParentId.Value)));
            return roots;
        }

        /// <summary>Fills a DropDownList with categories ("↳" marks sub-categories).</summary>
        public static void Fill(DropDownList ddl, string firstItemText, string firstItemValue)
        {
            ddl.Items.Clear();
            if (firstItemText != null)
                ddl.Items.Add(new ListItem(firstItemText, firstItemValue ?? ""));

            foreach (var cat in GetTree())
            {
                ddl.Items.Add(new ListItem(cat.Name, cat.Id.ToString()));
                foreach (var sub in cat.SubCategories)
                    ddl.Items.Add(new ListItem("   ↳ " + sub.Name, sub.Id.ToString()));
            }
        }

        /// <summary>Returns the id of the category with this name, creating it if needed.</summary>
        public static int FindOrCreate(string name)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0) throw new ArgumentException("Category name is required.");
            if (name.Length > 100) name = name.Substring(0, 100);

            var existing = Db.Scalar("SELECT TOP 1 Id FROM Categories WHERE Name = @n ORDER BY IsActive DESC, Id",
                Db.P("@n", name));
            if (existing != null)
            {
                var id = Convert.ToInt32(existing);
                Db.Exec("UPDATE Categories SET IsActive = 1 WHERE Id = @id", Db.P("@id", id));
                return id;
            }

            return Db.ScalarInt(
                @"INSERT INTO Categories (Name, ParentId, Description, IsActive)
                  VALUES (@n, NULL, @d, 1);
                  SELECT CAST(SCOPE_IDENTITY() AS INT);",
                Db.P("@n", name),
                Db.P("@d", "Category created from the admin panel: " + name));
        }
    }
}
