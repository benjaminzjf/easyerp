﻿namespace EasyERP.Desktop.Product
{
    using Caliburn.Micro;
    using Doamin.Service;
    using Domain.Model;
    using EasyERP.Desktop.Contacts;
    using EasyERP.Desktop.Extensions;
    using Infrastructure;
    using NullGuard;
    using PropertyChanged;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Dynamic;
    using System.Linq;
    using System.Windows;

    [ImplementPropertyChanged]
    public class ProductListViewModel : Screen, IViewModel
    {
        private readonly CategoryService categoryService;

        private readonly ProductService productService;

        public ProductListViewModel(ProductService productService, CategoryService categoryService)
        {
            this.productService = productService;
            this.categoryService = categoryService;
        }

        public override string DisplayName
        {
            get { return "Product Management"; }
            set { }
        }

        [AllowNull]
        public string SearchProductName { get; set; }

        public int SearchCategoryId { get; set; }

        [AllowNull]
        public IList<string> CategoryList { get; set; }

        public bool SearchIncludeSubCategories { get; set; }

        [AllowNull]
        public IList<string> ManufactureList { get; set; }

        public int SearchManufacturerId { get; set; }

        [AllowNull]
        public IList<string> StoreList { get; set; }

        public int SearchStoreId { get; set; }

        [AllowNull]
        public IList<string> VendorList { get; set; }

        public int SearchVendorId { get; set; }

        [AllowNull]
        public IList<string> WarehouseList { get; set; }

        public int SearchWarehouseId { get; set; }

        [AllowNull]
        public IList<string> ProductTypeList { get; set; }

        public int SearchProductTypeId { get; set; }

        public int SearchPublishedId { get; set; }

        [AllowNull]
        public string GoDirectlyToSku { get; set; }

        public ObservableCollection<ProductModel> Products
        {
            get
            {
                return
                    new ObservableCollection<ProductModel>(
                        this.productService.GetAllProducts().Select(this.PrepareProductModel));
            }
        }

        public string Tag
        {
            get { return "ProductManagement"; }
        }

        private ProductModel PrepareProductModel(Product product)
        {
            var model = product.ToModel();

            // get related price
            var prices = this.productService.GetPricesByProductId(model.Id);

            //model.Prices = prices.Select(p => p.ToModel()).ToList();

            if (prices.Any())
            {
                model.Price = prices.Aggregate(
                    (latest, price) =>
                    (latest == null || latest.UpdataTime > price.UpdataTime ? latest : price))
                                    .IfNotNull(p => p.SalePrice);
            }
            return model;
        }

        public void AddProduct()
        {
            var categories = this.categoryService.GetAllCategories();
            var edit = new EditProductViewModel
            {
                Product = new ProductModel
                {
                    Id = Guid.NewGuid()
                },
                Categories = new ObservableCollection<string>(categories.Select(c => c.Name).ToList())
            };

            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.AllowsTransparency = false;

            var result = IoC.Get<IWindowManager>().ShowDialog(edit, null, settings);

            //this.categoryService.GetProductCategoriesByProductId();

            if (result)
            {
                var entity = edit.Product.ToEntity();
                var category = categories.FirstOrDefault(i => i.Name == edit.Product.Category);
                if (category == null)
                {
                    category = new Category
                    {
                        Name = edit.Product.Category,
                        Descriiption = edit.Product.Category,
                        Id = Guid.NewGuid()
                    };
                    this.categoryService.InsertCategory(category);
                }
                var productCategory = new ProductCategory
                {
                    Id = Guid.NewGuid(),
                    ProductId = edit.Product.Id,
                    DisplayOrder = 1,
                    CategoryId = category.Id
                };
                this.productService.AddNewProduct(entity);
                this.categoryService.InsertProductCategory(productCategory);
            }
        }

        public void Delete()
        {
            // get all categories
            var edit = new EditProductViewModel
            {
                Product = new ProductModel()
            };

            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.AllowsTransparency = false;

            IoC.Get<IWindowManager>().ShowPopup(edit, null, settings);
        }

        public void Import()
        {
            var edit = new EditProductViewModel
            {
                Product = new ProductModel()
            };

            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.AllowsTransparency = false;

            IoC.Get<IWindowManager>().ShowWindow(edit, null, settings);
        }

        public void Export()
        {
        }

        public void GoToSku()
        {
            //var product = this.productService.GetProductBySku(this.GoDirectlyToSku);
        }
    }
}