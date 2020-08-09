﻿using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Settings
{
    [Validator(typeof(MediaSettingsValidator))]
    public class MediaSettingsModel : ModelBase
    {
        public MediaSettingsModel()
        {
            AvailablePictureZoomTypes = new List<SelectListItem>();
        }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Media.AutoGenerateAbsoluteUrls")]
		public bool AutoGenerateAbsoluteUrls { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Media.AvatarPictureSize")]
        public int AvatarPictureSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.ProductThumbPictureSize")]
        public int ProductThumbPictureSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.ProductDetailsPictureSize")]
        public int ProductDetailsPictureSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.ProductThumbPictureSizeOnProductDetailsPage")]
        public int ProductThumbPictureSizeOnProductDetailsPage { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Media.MessageProductThumbPictureSize")]
		public int MessageProductThumbPictureSize { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Media.AssociatedProductPictureSize")]
        public int AssociatedProductPictureSize { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Media.BundledProductPictureSize")]
		public int BundledProductPictureSize { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Media.CategoryThumbPictureSize")]
        public int CategoryThumbPictureSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.ManufacturerThumbPictureSize")]
        public int ManufacturerThumbPictureSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.CartThumbPictureSize")]
        public int CartThumbPictureSize { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Media.CartThumbBundleItemPictureSize")]
		public int CartThumbBundleItemPictureSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.MiniCartThumbPictureSize")]
        public int MiniCartThumbPictureSize { get; set; }
        
        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.MaximumImageSize")]
        public int MaximumImageSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.MaxUploadFileSize")]
        public long MaxUploadFileSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.MakeFilesTransientWhenOrphaned")]
        public bool MakeFilesTransientWhenOrphaned { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.DefaultPictureZoomEnabled")]
        public bool DefaultPictureZoomEnabled { get; set; }

        // (window || inner || lens)
        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.PictureZoomType")]
        public string PictureZoomType { get; set; }

        public List<SelectListItem> AvailablePictureZoomTypes { get; set; }

        #region Media types

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.Type.Image")]
        public string ImageTypes { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.Type.Video")]
        public string VideoTypes { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.Type.Audio")]
        public string AudioTypes { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.Type.Document")]
        public string DocumentTypes { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.Type.Text")]
        public string TextTypes { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.Type.Bin")]
        public string BinTypes { get; set; }

        #endregion

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.StorageProvider")]
		public string StorageProvider { get; set; }
		public List<SelectListItem> AvailableStorageProvider { get; set; }
	}

    public partial class MediaSettingsValidator : AbstractValidator<MediaSettingsModel>
    {
        public MediaSettingsValidator()
        {
            RuleFor(x => x.MaxUploadFileSize).GreaterThan(0);
        }
    }
}