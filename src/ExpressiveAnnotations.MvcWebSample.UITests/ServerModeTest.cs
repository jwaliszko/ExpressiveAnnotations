using System;
using System.Globalization;
using OpenQA.Selenium;
using Xunit;

namespace ExpressiveAnnotations.MvcWebSample.UITests
{
    public class ServerModeTest : BaseTest
    {
        public ServerModeTest(DriverFixture classContext, ServerFixture assemblyContext)
            : base(classContext, assemblyContext)
        {
            Home.SetMode("server");
        }

        protected override void Dispose(bool disposing)
        {
            Assert.Equal(1, Home.GetPostbacksCount());
            base.Dispose(disposing);
        }

        [Fact]
        public void unselect_go_abroad_and_verify_error_for_blood_in_server_mode()
        {
            Home.ClickCheckbox("GoAbroad");
            Home.Submit();
            Assert.Equal(
                "Blood type is required if you do extreme sports, or if you do any type of sport and plan to go abroad.", 
                Home.GetErrorMessage("BloodType"));
        }

        [Fact]
        public void change_culture_unselect_go_abroad_and_verify_error_for_blood_in_server_mode()
        {
            Home.SetLang("pl");
            Home.ClickCheckbox("GoAbroad");
            Home.Submit();
            Assert.Equal(
                "Typ krwi jest wymagany jeśli uprawiasz sporty ekstremalne, lub planujesz wyjechać za granicę i uprawiasz jakikolwiek sport.", 
                Home.GetErrorMessage("BloodType"));
        }

        [Fact]
        public void select_age_and_verify_error_and_its_extracted_tokens_for_travel_reason_in_server_mode()
        {
            Home.Select("Age", "15");
            Home.Submit();
            Assert.Equal(
                "If you are under 18 (indicated 15 in Age field, yes - Age 15), give us a reason of your travel no matter where you go (BTW. Poland... nice choice).",
                Home.GetErrorMessage("ReasonForTravel"));
        }

        [Fact]
        public void change_culture_select_age_and_verify_error_and_its_extracted_tokens_for_travel_reason_in_server_mode()
        {
            Home.SetLang("pl");
            Home.Select("Age", "15");
            Home.Submit();
            Assert.Equal(
                "Jeśli planujesz jakąkolwiek podróż i nie masz ukończonych 18 lat (wskazałeś w polu Wiek wartość 15, tak - Wiek 15), podaj powody (PS. Poland... dobry wybór).",
                Home.GetErrorMessage("ReasonForTravel"));
        }

        [Fact]
        public void write_complex_text_with_escaped_characters_in_travel_reason_and_verify_error_for_travel_reason_in_server_mode()
        {
            const string text = @"Simon's cat named ""\\""
 (Double Backslash)";
            Home.WriteTextarea("ReasonForTravel", text);
            Home.Submit();
            Assert.Equal(
                "Sorry, it is not a question about John's cat nor Simon's cat.",
                Home.GetErrorMessage("ReasonForTravel"));
        }

        [Fact]
        public void select_same_country_twice_and_verify_error_for_travel_reason_in_server_mode()
        {
            Home.Select("NextCountry", "Poland");
            Home.Submit();
            Assert.Equal(
                "If you plan to go abroad and you are between 25 and 55 or plan to visit the same foreign country twice, write down your reasons.",
                Home.GetErrorMessage("ReasonForTravel"));
        }

        [Fact]
        public void select_same_country_twice_write_text_in_travel_reason_and_verify_no_error_for_travel_reason_in_server_mode()
        {
            Home.Select("NextCountry", "Poland");
            Home.WriteTextarea("ReasonForTravel", "a");
            Home.Submit();
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ReasonForTravel"));
        }

        [Fact]
        public void select_same_country_twice_write_empty_text_in_travel_reason_and_verify_error_for_travel_reason_in_server_mode()
        {
            Home.Select("NextCountry", "Poland");
            Home.WriteTextarea("ReasonForTravel", Keys.Enter);
            Home.Submit();
            Assert.Equal(
                "If you plan to go abroad and you are between 25 and 55 or plan to visit the same foreign country twice, write down your reasons.",
                Home.GetErrorMessage("ReasonForTravel"));
        }

        [Fact]
        public void select_boundary_date_and_verify_no_error_for_long_travel_reason_in_server_mode()
        {
            Home.WriteInput("ReturnDate", DateTime.Today.AddMonths(1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));
            Home.Submit();
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ReasonForLongTravel"));
        }

        [Fact]
        public void select_slightly_too_late_date_and_verify_error_for_long_travel_reason_in_server_mode()
        {
            Home.WriteInput("ReturnDate", DateTime.Today.AddMonths(1).AddDays(1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));
            Home.Submit();
            Assert.Equal(
                "If you plan to stay abroad longer than one month from now, write down your reasons.",
                Home.GetErrorMessage("ReasonForLongTravel"));
        }

        [Fact]
        public void select_slightly_too_late_date_write_text_in_long_travel_reason_and_verify_no_error_for_long_travel_reason_in_server_mode()
        {
            Home.WriteInput("ReturnDate", DateTime.Today.AddMonths(1).AddDays(1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));
            Home.WriteTextarea("ReasonForLongTravel", "a");
            Home.Submit();
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ReasonForLongTravel"));
        }

        [Fact]
        public void select_slightly_too_late_date_write_empty_text_in_long_travel_reason_and_verify_no_error_for_long_travel_reason_in_server_mode()
        {
            Home.WriteInput("ReturnDate", DateTime.Today.AddMonths(1).AddDays(1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));
            Home.WriteTextarea("ReasonForLongTravel", Keys.Enter);
            Home.Submit();
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ReasonForLongTravel"));
        }

        [Fact]
        public void change_culture_select_boundary_date_and_verify_no_error_for_long_travel_reason_in_server_mode()
        {
            Home.SetLang("pl");
            Home.WriteInput("ReturnDate", DateTime.Today.AddMonths(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            Home.Submit();
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ReasonForLongTravel"));
        }

        [Fact]
        public void change_culture_select_slightly_too_late_date_and_verify_error_for_long_travel_reason_in_server_mode()
        {
            Home.SetLang("pl");
            Home.WriteInput("ReturnDate", DateTime.Today.AddMonths(1).AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            Home.Submit();
            Assert.Equal(
                "Jeśli planujesz przebywać za granicą dłużej niż miesiąc, podaj powody.",
                Home.GetErrorMessage("ReasonForLongTravel"));
        }

        [Fact]
        public void select_past_date_and_verify_error_for_date_in_server_mode()
        {
            Home.WriteInput("ReturnDate", DateTime.Today.AddDays(-1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));
            Home.Submit();
            Assert.Equal(
                "We are afraid that going back to the past is not an option nowadays.",
                Home.GetErrorMessage("ReturnDate"));
        }

        [Fact]
        public void select_today_and_verify_error_for_date_in_server_mode()
        {
            Home.WriteInput("ReturnDate", DateTime.Today.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));
            Home.Submit();
            Assert.Equal(
                "The period of stay should be at least a week.",
                Home.GetErrorMessage("ReturnDate"));
        }

        [Fact]
        public void select_six_days_from_today_and_verify_error_for_date_in_server_mode()
        {
            Home.WriteInput("ReturnDate", DateTime.Today.AddDays(6).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));
            Home.Submit();
            Assert.Equal(
                "The period of stay should be at least a week.",
                Home.GetErrorMessage("ReturnDate"));
        }

        [Fact]
        public void select_a_week_from_today_and_verify_no_error_for_date_in_server_mode()
        {
            Home.WriteInput("ReturnDate", DateTime.Today.AddDays(7).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));
            Home.Submit();
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ReturnDate"));
        }

        [Fact]
        public void select_a_year_without_day_from_today_and_verify_no_error_for_date_in_server_mode()
        {
            Home.WriteInput("ReturnDate", DateTime.Today.AddYears(1).AddDays(-1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));
            Home.Submit();
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ReturnDate"));
        }

        [Fact]
        public void select_a_year_from_today_and_verify_error_for_date_in_server_mode()
        {
            Home.WriteInput("ReturnDate", DateTime.Today.AddYears(1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));
            Home.Submit();
            Assert.Equal(
                "The period of stay should be less than a year.",
                Home.GetErrorMessage("ReturnDate"));
        }

        [Fact]
        public void write_nonsense_in_date_and_verify_error_for_date_in_server_mode()
        {
            Home.WriteInput("ReturnDate", "/");
            Home.Submit();            
            Assert.Equal(
                "The value '/' is not valid for Return date.", // 3rd party message
                Home.GetErrorMessage("ReturnDate"));
        }

        [Fact]
        public void select_uncertain_stability_and_verify_error_for_risks_in_server_mode()
        {
            Home.Select("PoliticalStability", "Uncertain");
            Home.Submit();
            Assert.Equal(
                "You are required to agree that you are aware of the risks of travel.",
                Home.GetErrorMessage("AwareOfTheRisks"));
        }

        [Fact]
        public void select_uncertain_stability_accept_risks_and_verify_no_error_for_risks_in_server_mode()
        {
            Home.ClickCheckbox("AwareOfTheRisks");
            Home.Select("PoliticalStability", "Uncertain");
            Home.Submit();
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("AwareOfTheRisks"));
        }

        [Fact]
        public void select_none_sport_and_verify_no_error_for_blood_in_server_mode()
        {
            Home.ClickRadio("SportType", "None");
            Home.Submit();
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("BloodType"));
        }

        [Fact]
        public void select_normal_sport_and_verify_error_for_blood_in_server_mode()
        {
            Home.ClickRadio("SportType", "Normal");
            Home.Submit();
            Assert.Equal(
                "Blood type is required if you do extreme sports, or if you do any type of sport and plan to go abroad.",
                Home.GetErrorMessage("BloodType"));
        }

        [Fact]
        public void write_incorrect_prefix_in_home_address_and_verify_error_for_home_address_in_server_mode()
        {
            Home.WriteInput("ContactDetails_Addresses_0__Details", "street");
            Home.Submit();
            Assert.Equal(
                "Address format should start from 'Street' prefix.",
                Home.GetErrorMessage("ContactDetails.Addresses[0].Details"));
        }

        [Fact]
        public void write_correct_prefix_in_home_address_and_verify_no_error_for_home_address_in_server_mode()
        {
            Home.WriteInput("ContactDetails_Addresses_0__Details", "Street");
            Home.Submit();
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ContactDetails.Addresses[0].Details"));
        }

        [Fact]
        public void change_culture_write_incorrect_prefix_in_home_address_and_verify_error_for_home_address_in_server_mode()
        {
            Home.SetLang("pl");
            Home.WriteInput("ContactDetails_Addresses_0__Details", "ulica");
            Home.Submit();
            Assert.Equal(
                "Format adresu powinien zaczynać się od prefixu 'Ulica'.",
                Home.GetErrorMessage("ContactDetails.Addresses[0].Details"));
        }

        [Fact]
        public void change_culture_write_correct_prefix_in_home_address_and_verify_no_error_for_home_address_in_server_mode()
        {
            Home.SetLang("pl");
            Home.WriteInput("ContactDetails_Addresses_0__Details", "Ulica");
            Home.Submit();
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ContactDetails.Addresses[0].Details"));
        }

        [Fact]
        public void submit_and_verify_error_for_contact_agreement_in_server_mode()
        {
            Home.Submit();
            Assert.Equal(
                "You have to authorize us to contact you.",
                Home.GetErrorMessage("AgreeForContact"));
        }

        [Fact]
        public void agree_for_contact_and_verify_no_error_for_contact_agreement_in_server_mode()
        {
            Home.ClickRadio("AgreeForContact", "True");
            Home.Submit();
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("AgreeForContact"));
        }

        [Fact]
        public void write_email_write_home_address_agree_for_contact_and_verify_error_for_immediate_contact_in_server_mode()
        {
            Home.WriteInput("ContactDetails_Email", "a");
            Home.WriteInput("ContactDetails_Addresses_0__Details", "a");
            Home.ClickRadio("AgreeForContact", "True");
            Home.Submit();
            Assert.Equal(
                "Shall we contact immidiately?",
                Home.GetErrorMessage("ImmediateContact"));
        }

        [Fact]
        public void write_email_write_home_address_agree_for_contact_select_immediate_contact_to_false_and_verify_no_error_for_immediate_contact_in_server_mode()
        {
            Home.WriteInput("ContactDetails_Email", "a");
            Home.WriteInput("ContactDetails_Addresses_0__Details", "a");
            Home.ClickRadio("AgreeForContact", "True");
            Home.Select("ImmediateContact", "No");
            Home.Submit();
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ImmediateContact"));
        }

        [Fact]
        public void submit_and_verify_error_for_donations_in_server_mode()
        {
            Home.Submit();
            Assert.Equal(
                "The Donation [USD] field is required by the following logic: GoAbroad == true",
                Home.GetErrorMessage("SelectedDonations"));
        }

        [Fact]
        public void select_one_donation_and_verify_error_for_donations_in_server_mode()
        {
            Home.ClickCheckbox("SelectedDonations", "1");
            Home.Submit();
            Assert.Equal(
                "At least two separate donations are required.",
                Home.GetErrorMessage("SelectedDonations"));
        }

        [Fact]
        public void select_two_donations_and_verify_no_error_for_donations_in_server_mode()
        {
            Home.ClickCheckbox("SelectedDonations", "1");
            Home.ClickCheckbox("SelectedDonations", "4");
            Home.Submit();
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("SelectedDonations"));
        }

        [Fact]
        public void write_letter_in_phone_and_verify_error_for_phone_in_server_mode()
        {
            Home.WriteInput("ContactDetails_Phone", "a");
            Home.Submit();
            Assert.Equal(
                "Only digits are accepted.",
                Home.GetErrorMessage("ContactDetails.Phone"));
        }

        [Fact]
        public void write_digit_in_phone_and_verify_error_for_phone_in_server_mode()
        {
            Home.WriteInput("ContactDetails_Phone", "1");
            Home.Submit();
            Assert.Equal(
                "Phone number should contain from 9 to 15 digits.",
                Home.GetErrorMessage("ContactDetails.Phone"));
        }

        [Fact]
        public void select_go_abroad_and_verify_error_for_comment_in_server_mode()
        {
            Home.Submit();
            Assert.Equal(
                "The Comment field is conditionally required.",
                Home.GetErrorMessage("Comment"));
        }

        [Fact]
        public void change_culture_select_go_abroad_and_verify_error_for_comment_in_server_mode()
        {
            Home.SetLang("pl");
            Home.Submit();
            Assert.Equal(
                "Pole Komentarz jest warunkowo wymagane.",
                Home.GetErrorMessage("Comment"));
        }

        [Fact]
        public void write_letters_in_comment_field_and_verify_error_for_comment_in_server_mode()
        {
            Home.WriteInput("Comment", "asd");
            Home.Submit();
            Assert.Equal(
                "Assertion for Comment field is not satisfied - text 'asd' is too short.",
                Home.GetErrorMessage("Comment"));
        }

        [Fact]
        public void change_culture_write_letters_in_comment_field_and_verify_error_for_comment_in_server_mode()
        {
            Home.SetLang("pl");
            Home.WriteInput("Comment", "asd");
            Home.Submit();
            Assert.Equal(
                "Warunek dla pola Komentarz nie jest spełniony - tekst 'asd' jest za krótki.",
                Home.GetErrorMessage("Comment"));
        }
    }
}
