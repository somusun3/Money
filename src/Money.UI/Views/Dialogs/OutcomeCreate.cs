﻿using Money.Services;
using Money.ViewModels.Parameters;
using Money.Views.Navigation;
using Neptuo.Activators;
using Neptuo.Models.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Money.Views.Dialogs
{
    [NavigationParameter(typeof(OutcomeParameter))]
    public class OutcomeCreate : IWizard
    {
        private readonly IDomainFacade domainFacade = ServiceProvider.DomainFacade;

        public async Task ShowAsync(object context)
        {
            OutcomeParameter parameter = (OutcomeParameter)context;

            decimal amount = 0;
            string description = String.Empty;
            DateTime when = DateTime.Now;
            IKey categoryKey = parameter.CategoryKey;

            OutcomeAmount amountDialog = new OutcomeAmount();
            amountDialog.PrimaryButtonText = "Next";

            if (parameter.CategoryKey.IsEmpty)
                amountDialog.SecondaryButtonText = String.Empty;
            else
                amountDialog.SecondaryButtonText = "Create today";

            if (parameter.Amount != null)
                amountDialog.Value = (double)parameter.Amount.Value;

            ContentDialogResult result = await amountDialog.ShowAsync();
            if (result == ContentDialogResult.None)
                return;

            amount = (decimal)amountDialog.Value;
            if (result == ContentDialogResult.Primary)
            {
                OutcomeDescription descriptionDialog = new OutcomeDescription();
                descriptionDialog.PrimaryButtonText = "Next";

                if (parameter.CategoryKey.IsEmpty)
                    descriptionDialog.SecondaryButtonText = String.Empty;
                else
                    descriptionDialog.SecondaryButtonText = "Create today";

                result = await descriptionDialog.ShowAsync();
                if (result == ContentDialogResult.None && !descriptionDialog.IsEnterPressed)
                    return;

                description = descriptionDialog.Value;
                if (result == ContentDialogResult.Primary || descriptionDialog.IsEnterPressed)
                {
                    CategoryPicker categoryDialog = new CategoryPicker();
                    categoryDialog.PrimaryButtonText = "Next";
                    categoryDialog.SecondaryButtonText = "Create today";
                    if (!parameter.CategoryKey.IsEmpty)
                        categoryDialog.SelectedKey = parameter.CategoryKey;

                    result = await categoryDialog.ShowAsync();
                    if (result == ContentDialogResult.None)
                        return;

                    categoryKey = categoryDialog.SelectedKey;
                    if (result == ContentDialogResult.Primary)
                    {
                        OutcomeWhen whenDialog = new OutcomeWhen();
                        whenDialog.PrimaryButtonText = "Create";
                        whenDialog.SecondaryButtonText = "Cancel";
                        whenDialog.Value = when;

                        result = await whenDialog.ShowAsync();
                        if (result != ContentDialogResult.Primary)
                            return;

                        when = whenDialog.Value;
                    }
                }
            }

            await domainFacade.CreateOutcomeAsync(
                domainFacade.PriceFactory.Create(amount),
                description,
                when,
                categoryKey
            );

            //OutcomeCreatedGuidePost nextDialog = new OutcomeCreatedGuidePost();
            //await nextDialog.ShowAsync();
        }
    }
}
