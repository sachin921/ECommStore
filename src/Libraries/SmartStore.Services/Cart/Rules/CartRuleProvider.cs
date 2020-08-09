﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using SmartStore.Rules;
using SmartStore.Rules.Domain;
using SmartStore.Services.Cart.Rules.Impl;

namespace SmartStore.Services.Cart.Rules
{
    public interface ICartRuleProvider : IRuleProvider
    {
        IRule GetProcessor(RuleExpression expression);
        RuleExpressionGroup CreateExpressionGroup(int ruleSetId);

        bool RuleMatches(RuleExpression expression);
        bool RuleMatches(int[] ruleSetIds, LogicalRuleOperator logicalOperator);
        bool RuleMatches(IRulesContainer entity, LogicalRuleOperator logicalOperator = LogicalRuleOperator.Or);
        bool RuleMatches(RuleExpression[] expressions, LogicalRuleOperator logicalOperator);
    }

    public class CartRuleProvider : RuleProviderBase, ICartRuleProvider
    {
        private readonly IRuleFactory _ruleFactory;
        private readonly IComponentContext _componentContext;
        private readonly ICommonServices _services;

        public CartRuleProvider(IRuleFactory ruleFactory, IComponentContext componentContext, ICommonServices services)
            : base(RuleScope.Cart)
        {
            _ruleFactory = ruleFactory;
            _componentContext = componentContext;
            _services = services;
        }

        public RuleExpressionGroup CreateExpressionGroup(int ruleSetId)
        {
            return _ruleFactory.CreateExpressionGroup(ruleSetId, this) as RuleExpressionGroup;
        }

        public override IRuleExpression VisitRule(RuleEntity rule)
        {
            var expression = new RuleExpression();
            base.ConvertRule(rule, expression);
            return expression;
        }

        public override IRuleExpressionGroup VisitRuleSet(RuleSetEntity ruleSet)
        {
            var group = new RuleExpressionGroup
            {
                Id = ruleSet.Id,
                LogicalOperator = ruleSet.LogicalOperator,
                IsSubGroup = ruleSet.IsSubGroup,
                Value = ruleSet.Id,
                RawValue = ruleSet.Id.ToString(),
                Provider = this,
                Descriptor = new CartRuleDescriptor
                {
                    RuleType = RuleType.Boolean,
                    ProcessorType = typeof(CompositeRule)
                }
            };

            return group;
        }

        public bool RuleMatches(RuleExpression expression)
        {
            Guard.NotNull(expression, nameof(expression));

            return RuleMatches(new[] { expression }, LogicalRuleOperator.And);
        }

        public bool RuleMatches(int[] ruleSetIds, LogicalRuleOperator logicalOperator)
        {
            Guard.NotNull(ruleSetIds, nameof(ruleSetIds));

            if (ruleSetIds.Length == 0)
                return true;

            var expressions = ruleSetIds
                .Select(id => _ruleFactory.CreateExpressionGroup(id, this))
                .Where(x => x != null)
                .Cast<RuleExpression>()
                .ToArray();

            return RuleMatches(expressions, logicalOperator);
        }

        public bool RuleMatches(IRulesContainer entity, LogicalRuleOperator logicalOperator = LogicalRuleOperator.Or)
        {
            Guard.NotNull(entity, nameof(entity));

            var ruleSets = entity.RuleSets.Where(x => x.Scope == RuleScope.Cart).ToArray();
            if (!ruleSets.Any())
            {
                return true;
            }

            var expressions = ruleSets
                .Select(x => _ruleFactory.CreateExpressionGroup(x, this))
                .Where(x => x != null)
                .Cast<RuleExpression>()
                .ToArray();

            return RuleMatches(expressions, logicalOperator);
        }

        public bool RuleMatches(RuleExpression[] expressions, LogicalRuleOperator logicalOperator)
        {
            Guard.NotNull(expressions, nameof(expressions));

            if (expressions.Length == 0)
            {
                return true;
            }

            RuleExpressionGroup group;

            if (expressions.Length == 1 && expressions[0] is RuleExpressionGroup group2)
            {
                group = group2;
            }
            else
            {
                group = new RuleExpressionGroup { LogicalOperator = logicalOperator };
                group.AddExpressions(expressions);
            }

            var context = new CartRuleContext
            {
                Customer = _services.WorkContext.CurrentCustomer,
                Store = _services.StoreContext.CurrentStore,
                WorkContext = _services.WorkContext
            };

            var processor = GetProcessor(group);

            return processor.Match(context, group);
        }

        public IRule GetProcessor(RuleExpression expression)
        {
            var group = expression as RuleExpressionGroup;
            var descriptor = expression.Descriptor as CartRuleDescriptor;
            
            if (group == null && descriptor == null)
            {
                throw new InvalidOperationException($"Missing cart rule descriptor for expression {expression.Id} ('{expression.RawValue.EmptyNull()}').");
            }

            IRule instance;

            if (group == null && descriptor.ProcessorType != typeof(CompositeRule))
            {
                instance = _componentContext.ResolveKeyed<IRule>(descriptor.ProcessorType);
            }
            else
            {
                instance = new CompositeRule(group, this);
            }

            return instance;
        }

        protected override IEnumerable<RuleDescriptor> LoadDescriptors()
        {
            var language = _services.WorkContext.WorkingLanguage;

            var stores = _services.StoreService.GetAllStores()
                .Select(x => new RuleValueSelectListOption { Value = x.Id.ToString(), Text = x.Name })
                .ToArray();

            var descriptors = new List<CartRuleDescriptor>
            {
                new CartRuleDescriptor
                {
                    Name = "Currency",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.Currency"),
                    RuleType = RuleType.IntArray,
                    ProcessorType = typeof(CurrencyRule),
                    SelectList = new RemoteRuleValueSelectList("Currency") { Multiple = true }
                },
                new CartRuleDescriptor
                {
                    Name = "Language",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.Language"),
                    RuleType = RuleType.IntArray,
                    ProcessorType = typeof(LanguageRule),
                    SelectList = new RemoteRuleValueSelectList("Language") { Multiple = true }
                },
                new CartRuleDescriptor
                {
                    Name = "Store",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.Store"),
                    RuleType = RuleType.IntArray,
                    ProcessorType = typeof(StoreRule),
                    SelectList = new LocalRuleValueSelectList(stores) { Multiple = true }
                },
                new CartRuleDescriptor
                {
                    Name = "IPCountry",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.IPCountry"),
                    RuleType = RuleType.StringArray,
                    ProcessorType = typeof(IPCountryRule),
                    SelectList = new RemoteRuleValueSelectList("Country") { Multiple = true }
                },
                new CartRuleDescriptor
                {
                    Name = "Weekday",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.Weekday"),
                    RuleType = RuleType.IntArray,
                    ProcessorType = typeof(WeekdayRule),
                    SelectList = new LocalRuleValueSelectList(WeekdayRule.GetDefaultValues(language)) { Multiple = true }
                },
                new CartRuleDescriptor
                {
                    Name = "UserAgent.IsMobile",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.MobileDevice"),
                    RuleType = RuleType.Boolean,
                    ProcessorType = typeof(IsMobileRule)
                },
                new CartRuleDescriptor
                {
                    Name = "UserAgent.Device",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.DeviceFamily"),
                    RuleType = RuleType.StringArray,
                    ProcessorType = typeof(DeviceRule),
                    SelectList = new LocalRuleValueSelectList(DeviceRule.GetDefaultValues()) { Multiple = true, Tags = true }
                },
                new CartRuleDescriptor
                {
                    Name = "UserAgent.OS",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.OperatingSystem"),
                    RuleType = RuleType.StringArray,
                    ProcessorType = typeof(OSRule),
                    SelectList = new LocalRuleValueSelectList(OSRule.GetDefaultValues()) { Multiple = true, Tags = true }
                },
                new CartRuleDescriptor
                {
                    Name = "UserAgent.Browser",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.BrowserName"),
                    RuleType = RuleType.StringArray,
                    ProcessorType = typeof(BrowserRule),
                    SelectList = new LocalRuleValueSelectList(BrowserRule.GetDefaultValues()) { Multiple = true, Tags = true }
                },
                new CartRuleDescriptor
                {
                    Name = "UserAgent.BrowserMajorVersion",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.BrowserMajorVersion"),
                    RuleType = RuleType.Int,
                    ProcessorType = typeof(BrowserMajorVersionRule)
                },
                new CartRuleDescriptor
                {
                    Name = "UserAgent.BrowserMinorVersion",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.BrowserMinorVersion"),
                    RuleType = RuleType.Int,
                    ProcessorType = typeof(BrowserMinorVersionRule)
                },

                new CartRuleDescriptor
                {
                    Name = "CustomerRole",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.IsInCustomerRole"),
                    RuleType = RuleType.IntArray,
                    ProcessorType = typeof(CustomerRoleRule),
                    SelectList = new RemoteRuleValueSelectList("CustomerRole") { Multiple = true },
                    IsComparingSequences = true
                },
                new CartRuleDescriptor
                {
                    Name = "CartTotal",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.CartTotal"),
                    RuleType = RuleType.Money,
                    ProcessorType = typeof(CartTotalRule)
                },
                new CartRuleDescriptor
                {
                    Name = "CartSubtotal",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.CartSubtotal"),
                    RuleType = RuleType.Money,
                    ProcessorType = typeof(CartSubtotalRule)
                },
                new CartRuleDescriptor
                {
                    Name = "ProductInCart",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.ProductInCart"),
                    RuleType = RuleType.IntArray,
                    ProcessorType = typeof(ProductInCartRule),
                    SelectList = new RemoteRuleValueSelectList("Product") { Multiple = true },
                    IsComparingSequences = true
                },
                new CartRuleDescriptor
                {
                    Name = "CartProductCount",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.CartProductCount"),
                    RuleType = RuleType.Int,
                    ProcessorType = typeof(CartProductCountRule)
                },
                new CartRuleDescriptor
                {
                    Name = "ProductFromCategoryInCart",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.ProductFromCategoryInCart"),
                    RuleType = RuleType.IntArray,
                    ProcessorType = typeof(ProductFromCategoryInCartRule),
                    SelectList = new RemoteRuleValueSelectList("Category") { Multiple = true },
                    IsComparingSequences = true
                },
                new CartRuleDescriptor
                {
                    Name = "ProductFromManufacturerInCart",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.ProductFromManufacturerInCart"),
                    RuleType = RuleType.IntArray,
                    ProcessorType = typeof(ProductFromManufacturerInCartRule),
                    SelectList = new RemoteRuleValueSelectList("Manufacturer") { Multiple = true },
                    IsComparingSequences = true
                },
                new CartRuleDescriptor
                {
                    Name = "ProductInWishlist",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.ProductOnWishlist"),
                    RuleType = RuleType.IntArray,
                    ProcessorType = typeof(ProductOnWishlistRule),
                    SelectList = new RemoteRuleValueSelectList("Product") { Multiple = true },
                    IsComparingSequences = true
                },
                new CartRuleDescriptor
                {
                    Name = "ProductReviewCount",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.ProductReviewCount"),
                    RuleType = RuleType.Int,
                    ProcessorType = typeof(ProductReviewCountRule)
                },
                new CartRuleDescriptor
                {
                    Name = "RewardPointsBalance",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.RewardPointsBalance"),
                    RuleType = RuleType.Int,
                    ProcessorType = typeof(RewardPointsBalanceRule)
                },
                new CartRuleDescriptor
                {
                    Name = "CartBillingCountry",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.BillingCountry"),
                    RuleType = RuleType.IntArray,
                    ProcessorType = typeof(BillingCountryRule),
                    SelectList = new RemoteRuleValueSelectList("Country") { Multiple = true }
                },
                new CartRuleDescriptor
                {
                    Name = "CartShippingCountry",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.ShippingCountry"),
                    RuleType = RuleType.IntArray,
                    ProcessorType = typeof(ShippingCountryRule),
                    SelectList = new RemoteRuleValueSelectList("Country") { Multiple = true }
                },
                new CartRuleDescriptor
                {
                    Name = "CartShippingMethod",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.ShippingMethod"),
                    RuleType = RuleType.IntArray,
                    ProcessorType = typeof(ShippingMethodRule),
                    SelectList = new RemoteRuleValueSelectList("ShippingMethod") { Multiple = true }
                },
                new CartRuleDescriptor
                {
                    Name = "CartOrderCount",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.OrderCount"),
                    RuleType = RuleType.Int,
                    ProcessorType = typeof(OrderCountRule)
                },
                new CartRuleDescriptor
                {
                    Name = "CartSpentAmount",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.SpentAmount"),
                    RuleType = RuleType.Money,
                    ProcessorType = typeof(SpentAmountRule)
                },
                new CartRuleDescriptor
                {
                    Name = "CartPurchasedProduct",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.PurchasedProduct"),
                    RuleType = RuleType.IntArray,
                    ProcessorType = typeof(PurchasedProductRule),
                    SelectList = new RemoteRuleValueSelectList("Product") { Multiple = true },
                    IsComparingSequences = true
                },
                new CartRuleDescriptor
                {
                    Name = "CartPurchasedFromManufacturer",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.PurchasedFromManufacturer"),
                    RuleType = RuleType.IntArray,
                    ProcessorType = typeof(PurchasedFromManufacturerRule),
                    SelectList = new RemoteRuleValueSelectList("Manufacturer") { Multiple = true },
                    IsComparingSequences = true
                },
                new CartRuleDescriptor
                {
                    Name = "CartPaymentMethod",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.PaymentMethod"),
                    RuleType = RuleType.StringArray,
                    ProcessorType = typeof(PaymentMethodRule),
                    SelectList = new RemoteRuleValueSelectList("PaymentMethod") { Multiple = true }
                },
                new CartRuleDescriptor
                {
                    Name = "CartPaidBy",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.PaidBy"),
                    RuleType = RuleType.StringArray,
                    ProcessorType = typeof(PaidByRule),
                    SelectList = new RemoteRuleValueSelectList("PaymentMethod") { Multiple = true },
                    IsComparingSequences = true
                },
                new CartRuleDescriptor
                {
                    Name = "RuleSet",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.RuleSet"),
                    RuleType = RuleType.Int,
                    ProcessorType = typeof(RuleSetRule),
                    Operators = new[] { RuleOperator.IsEqualTo, RuleOperator.IsNotEqualTo },
                    SelectList = new RemoteRuleValueSelectList("CartRule"),
                }
            };

            descriptors
                .Where(x => x.RuleType == RuleType.Money)
                .Each(x => x.Metadata["postfix"] = _services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode);

            return descriptors;
        }
    }
}
