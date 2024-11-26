
// (c) 2024 Kazuki KOHZUKI

using TAFitting.Model;

namespace TAFitting.Controls.LinearCombination;

[DesignerCategory("Code")]
internal sealed partial class LinearCombinationEditWindow : Form
{
    private readonly TextBox tb_name;
    private readonly ComboBox cb_categoryFilter, cb_model, cb_newCategory;
    private readonly ModelsTable modelsTable;
    private readonly Button btn_addModel, btn_register;

    internal LinearCombinationEditWindow()
    {
        this.Text = "Add linear combination model";
        this.Size = this.MinimumSize = this.MaximumSize = new Size(440, 440);
        this.SizeGripStyle = SizeGripStyle.Hide;

        _ = new Label()
        {
            Text = "Filter:",
            Location = new Point(10, 10),
            Width = 100,
            Parent = this,
        };

        this.cb_categoryFilter = new()
        {
            Location = new Point(110, 10),
            Width = 250,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Parent = this,
        };
        this.cb_categoryFilter.Items.Add(new ModelCategoryItem("All") { IsDefined = false });
        var categories =
            ModelManager.Models.Aggregate(
                new HashSet<string>(), (set, item) => { set.Add(item.Value.Category); return set; }
            ).Select(c => new ModelCategoryItem(c))
            .ToArray();
        this.cb_categoryFilter.Items.AddRange(categories);

        _ = new Label()
        {
            Text = "Model:",
            Location = new Point(10, 40),
            Width = 100,
            Parent = this,
        };

        this.cb_model = new()
        {
            Location = new Point(110, 40),
            Width = 250,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Parent = this,
        };

        this.btn_addModel = new Button()
        {
            Text = "Add",
            Enabled = false,
            Location = new Point(370, 40),
            Size = new Size(40, this.cb_model.Height),
            Parent = this,
        };
        this.btn_addModel.Click += AddModel;

        this.cb_model.SelectedIndexChanged += (sender, e) => this.btn_addModel.Enabled = this.cb_model.SelectedIndex >= 0;
        this.cb_categoryFilter.SelectedIndexChanged += UpdateModelsList;
        this.cb_categoryFilter.SelectedIndex = 0;

        this.modelsTable = new()
        {
            Location = new Point(10, 80),
            Size = new Size(400, 200),
            Parent = this,
        };

        _ = new Label()
        {
            Text = "Name:",
            Location = new Point(10, 300),
            Width = 100,
            Parent = this,
        };

        this.tb_name = new TextBox()
        {
            Location = new Point(110, 300),
            Width = 300,
            Parent = this,
        };

        _ = new Label()
        {
            Text = "Category:",
            Location = new Point(10, 330),
            Width = 100,
            Parent = this,
        };

        this.cb_newCategory = new()
        {
            Location = new Point(110, 330),
            Width = 300,
            Parent = this,
        };
        this.cb_newCategory.Items.AddRange(categories);

        this.btn_register = new()
        {
            Text = "Register",
            Location = new Point(10, 360),
            Size = new(400, 30),
            Enabled = false,
            Parent = this,
        };
        this.btn_register.Click += RegisterModel;

        this.tb_name.TextChanged += ValidateInput;
        this.modelsTable.RowsAdded += ValidateInput;
        this.modelsTable.RowsRemoved += ValidateInput;
    } // ctor ()

    private void UpdateModelsList(object? sender, EventArgs e)
        => UpdateModelsList();

    private void UpdateModelsList()
    {
        if (this.cb_categoryFilter.SelectedItem is not ModelCategoryItem category) return;
        this.cb_model.Items.Clear();
        var models = category.IsDefined
            ? ModelManager.Models.Where(item => item.Value.Category == category.Category)
            : ModelManager.Models;
        this.cb_model.Items.AddRange(models.Select(m => m.Value).Where(m => m.Model is IAnalyticallyDifferentiable).ToArray());
        this.btn_addModel.Enabled = false;
    } // private void UpdateModelsList ()

    private void AddModel(object? sender, EventArgs e)
    {
        if (this.cb_model.SelectedItem is not ModelItem model) return;
        this.modelsTable.AddModel(model);
    } // private void AddModel (object, EventArgs)

    private void ValidateInput(object? sender, EventArgs e)
        => this.btn_register.Enabled = ValidateInput();

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(this.tb_name.Text)) return false;
        if (this.modelsTable.ModelRows.Count() < 2) return false;
        return true;
    } // private bool ValidateInput ()

    private void RegisterModel(object? sender, EventArgs e)
        => RegisterModel();

    private void RegisterModel()
    {
        if (!ValidateInput()) return;

        var guid = Guid.NewGuid();
        var name = this.tb_name.Text;
        var category = this.cb_newCategory.Text;
        var components = this.modelsTable.ModelRows.Select(row => row.Model.GetType().GUID).ToArray();
        var item = Program.AddLinearCombination(guid, name, category, components);
        item.Register();
    } // private void RegisterModel ()
} // internal sealed partial class LinearCombinationEditWindow : Form
