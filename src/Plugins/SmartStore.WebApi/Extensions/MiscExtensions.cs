﻿using System.Text;
using System.Web.Http.OData;
using System.Web.Mvc;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Localization;

namespace SmartStore.WebApi
{
	public static class PublicMiscExtensions
	{
		public static string GridApiInfo<T>(this HtmlHelper<T> helper)
		{
			var sb = new StringBuilder();
			var localize = EngineContext.Current.Resolve<ILocalizationService>();

			// API infos.
			var rowTemplate = "<div style='display:table-row'><div style='display:table-cell'>{0}:&nbsp;</div><div style='display:table-cell'>{1}</div></div>";

			sb.Append("<div style='display:<#= DisplayApiInfo #>; padding:1px 3px;'>");
			sb.AppendFormat(rowTemplate, localize.GetResource("Plugins.Api.WebApi.PublicKey"), "<#= PublicKey #>");
			sb.AppendFormat(rowTemplate, localize.GetResource("Plugins.Api.WebApi.SecretKey"), "<#= SecretKey #>");
			sb.AppendFormat(rowTemplate, localize.GetResource("Plugins.Api.WebApi.ApiEnabled"), "<#= EnabledFriendly #>");
			sb.AppendFormat(rowTemplate, localize.GetResource("Plugins.Api.WebApi.LastRequest"), "<#= LastRequestDateFriendly #>");
			sb.Append("</div>");

			// Command buttons.
			var buttonTemplate = "<button name='{0}' class='btn {1} api-grid-button mr-1' style='display: {2};'><i class='{3}'></i><span>{4}</span></button>";

			sb.Append("<div data-id='<#= Id #>' class='mt-2'>");
			sb.AppendFormat(buttonTemplate, "ApiButtonRemoveKeys", "btn-danger", "<#= ButtonDisplayRemoveKeys #>", "far fa-trash-alt", localize.GetResource("Plugins.Api.WebApi.RemoveKeys"));
			sb.AppendFormat(buttonTemplate, "ApiButtonCreateKeys", "btn-info", "<#= ButtonDisplayCreateKeys #>", "fa fa-check", localize.GetResource("Plugins.Api.WebApi.CreateKeys"));
			sb.AppendFormat(buttonTemplate, "ApiButtonEnable", "", "<#= ButtonDisplayEnable #>", "fa fa-unlock", localize.GetResource("Plugins.Api.WebApi.Activate"));
			sb.AppendFormat(buttonTemplate, "ApiButtonDisable", "", "<#= ButtonDisplayDisable #>", "fa fa-lock", localize.GetResource("Plugins.Api.WebApi.Deactivate"));
			sb.Append("</div>");

			return sb.ToString();
		}

		public static T GetValueSafe<T>(this ODataActionParameters parameters, string key)
		{
			if (parameters != null && key.HasValue() && parameters.TryGetValue(key, out var value))
			{
				return value.Convert<T>();
			}

			return default(T);
		}
	}

	internal static class MiscExtensions
	{
		public static string ToUnquoted(this string value)
		{
			if (value.HasValue() && value.Length > 1)
			{
				if ((value.StartsWith("\"") && value.EndsWith("\"")) || (value.StartsWith("'") && value.EndsWith("'")))
					return value.Substring(1, value.Length - 2);
			}
			return value;
		}
	}
}
