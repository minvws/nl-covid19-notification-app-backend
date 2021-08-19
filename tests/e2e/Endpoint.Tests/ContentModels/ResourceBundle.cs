// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;

namespace Endpoint.Tests.ContentModels
{
    public class ResourceBundle
    {
        public Guidance guidance { get; set; }
        public Resources resources { get; set; }

        public List<BaseCountry> TestableResources { get; private set; }

        public void CreateTestableResourceBundle()
        {
            TestableResources = new List<BaseCountry>
            {
                resources.ar,
                resources.bg,
                resources.de,
                resources.en,
                resources.es,
                resources.fr,
                resources.fy,
                resources.nl,
                resources.pl,
                resources.ro,
                resources.tr
            };
        }


        public class Layout
        {
            public string body { get; set; }
            public string title { get; set; }
            public string type { get; set; }
        }

        public class LayoutByRelativeExposureDay
        {
            public int exposureDaysLowerBoundary { get; set; }
            public int exposureDaysUpperBoundary { get; set; }
            public List<Layout> layout { get; set; }
        }

        public class Guidance
        {
            public List<object> layout { get; set; }
            public List<LayoutByRelativeExposureDay> layoutByRelativeExposureDay { get; set; }
        }

        public class Resources
        {
            public Ar ar { get; set; }
            public Bg bg { get; set; }
            public De de { get; set; }
            public En en { get; set; }
            public Es es { get; set; }
            public Fr fr { get; set; }
            public Fy fy { get; set; }
            public Nl nl { get; set; }
            public Pl pl { get; set; }
            public Ro ro { get; set; }
            public Tr tr { get; set; }
        }

        public class Ar : BaseCountry
        {
        }

        public class Bg : BaseCountry
        {
        }

        public class De : BaseCountry
        {
        }

        public class En : BaseCountry
        {
        }

        public class Es : BaseCountry
        {
        }

        public class Fr : BaseCountry
        {
        }

        public class Fy : BaseCountry
        {
        }

        public class Nl : BaseCountry
        {
        }

        public class Pl : BaseCountry
        {
        }

        public class Ro : BaseCountry
        {
        }

        public class Tr : BaseCountry
        {
        }

        public class BaseCountry
        {
            public string about_this_notification_body { get; set; }
            public string about_this_notification_title { get; set; }
            public string advice_body { get; set; }
            public string advice_title { get; set; }
            public string dont_test_body { get; set; }
            public string dont_test_title { get; set; }
            public string exposure_notification_body { get; set; }
            public string exposure_notification_body_exposure_days_11_x { get; set; }
            public string exposure_notification_body_v1_legacy { get; set; }
            public string exposure_notification_title { get; set; }
            public string medical_help_body { get; set; }
            public string medical_help_title { get; set; }
            public string next_steps_body { get; set; }
            public string next_steps_body_exposure_days_11_x { get; set; }
            public string next_steps_body_exposure_days_1_10 { get; set; }
            public string next_steps_body_exposure_days_4_10 { get; set; }
            public string next_steps_body_exposure_days_x_3 { get; set; }
            public string next_steps_title { get; set; }
            public string next_steps_title_exposure_days_11_x { get; set; }
            public string stay_home_body { get; set; }
            public string stay_home_title { get; set; }
            public string symptoms_body { get; set; }
            public string symptoms_title { get; set; }
            public string test_negative_body { get; set; }
            public string test_negative_title { get; set; }
            public string vaccinated_body { get; set; }
            public string vaccinated_title { get; set; }
        }
    }
}
