using System;
using OpenQA.Selenium;
using Xunit;

namespace ExpressiveAnnotations.MvcWebSample.UITests
{
    public class ClientModeTest : BaseTest
    {
        public ClientModeTest(DriverFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void client_validation_switch_should_be_highlited_in_client_mode()
        {            
            Home.SetMode("client");
            Assert.Equal(
                "[client]", 
                Home.GetSelectedMode());
        }

        [Fact]
        public void unselect_goabroad_and_check_blood_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.ClickCheckbox("GoAbroad");
            Assert.Equal(
                "Blood type is required if you do extreme sports, or if you do any type of sport and plan to go abroad.", 
                Home.GetErrorMessage("BloodType"));
        }

        [Fact]
        public void change_culture_unselect_goabroad_and_check_blood_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.SetLang("pl");
            Home.ClickCheckbox("GoAbroad");
            Assert.Equal(
                "Typ krwi jest wymagany jeśli uprawiasz sporty ekstremalne, lub planujesz wyjechać za granicę i uprawiasz jakikolwiek sport.", 
                Home.GetErrorMessage("BloodType"));
        }

        [Fact]
        public void unselect_change_trigger_unselect_goabroad_and_check_no_blood_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.ClickTrigger("change");
            Home.ClickCheckbox("GoAbroad");
            Assert.Equal(
                string.Empty, 
                Home.GetErrorMessage("BloodType"));
        }

        [Fact]
        public void select_age_in_dropdown_and_verify_complex_travel_reason_in_client_mode()
        {
            Home.SetMode("client");
            Home.Select("Age", "15");
            Assert.Equal(
                "If you are under 18 (indicated 15 in Age field, yes - Age 15), give us a reason of your travel no matter where you go (BTW. Poland... nice choice).", 
                Home.GetErrorMessage("ReasonForTravel"));
        }

        [Fact]
        public void change_culture_select_age_in_dropdown_and_verify_processed_travel_reason_in_client_mode()
        {
            Home.SetMode("client");
            Home.SetLang("pl");
            Home.Select("Age", "15");
            Assert.Equal(
                "Jeśli planujesz jakąkolwiek podróż i nie masz ukończonych 18 lat (wskazałeś w polu Wiek wartość 15, tak - Wiek 15), podaj powody (PS. Poland... dobry wybór).",
                Home.GetErrorMessage("ReasonForTravel"));
        }

        [Fact]
        public void verify_input_with_escaped_characters_validation_in_client_mode()
        {
            const string text = @"Simon's cat named ""\\""
 (Double Backslash)";
            Home.SetMode("client");
            Home.WriteTextarea("ReasonForTravel", text);
            Assert.Equal(
                "Sorry, it is not a question about John's cat nor Simon's cat.",
                Home.GetErrorMessage("ReasonForTravel"));
        }

        [Fact]
        public void write_letter_and_verify_phone_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.WriteInput("ContactDetails_Phone", "a");
            Assert.Equal(
                "Only digits are accepted.",
                Home.GetErrorMessage("ContactDetails.Phone"));
        }

        [Fact]
        public void press_backspace_in_email_field_and_verify_phone_error_is_triggered_in_client_mode()
        {
            Home.SetMode("client");
            Home.WriteInput("ContactDetails_Email", Keys.Backspace);
            Assert.Equal(
                "You do not enter any contact information. At least e-mail or phone should be provided.",
                Home.GetErrorMessage("ContactDetails.Phone"));
        }

        [Fact]
        public void unselect_keyup_trigger_press_backspace_in_email_field_and_verify_no_phone_error_is_triggered_in_client_mode()
        {
            Home.SetMode("client");
            Home.ClickTrigger("keyup");
            Home.WriteInput("ContactDetails_Email", Keys.Backspace);
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ContactDetails.Phone"));
        }

        [Fact]
        public void select_same_country_twice_and_verify_reason_for_travel_in_client_mode()
        {
            Home.SetMode("client");
            Home.Select("NextCountry", "Poland");
            Assert.Equal(
                "If you plan to go abroad and you are between 25 and 55 or plan to visit the same foreign country twice, write down your reasons.",
                Home.GetErrorMessage("ReasonForTravel"));
        }

        [Fact]
        public void select_same_country_twice_then_deselect_and_verify_no_reason_for_travel_in_client_mode()
        {
            Home.SetMode("client");
            Home.Select("NextCountry", "Poland");
            Home.Select("NextCountry", "Germany");
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ReasonForTravel"));
        }

        [Fact]
        public void select_same_country_twice_put_text_in_reason_for_travel_verify_no_reason_for_travel_in_client_mode()
        {
            Home.SetMode("client");
            Home.Select("NextCountry", "Poland");
            Home.WriteTextarea("ReasonForTravel", "a");
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ReasonForTravel"));
        }

        [Fact]
        public void select_same_country_twice_put_empty_text_in_reason_for_travel_verify_unchanged_reason_for_travel_in_client_mode()
        {
            Home.SetMode("client");
            Home.Select("NextCountry", "Poland");
            Home.WriteTextarea("ReasonForTravel", Keys.Enter);
            Assert.Equal(
                "If you plan to go abroad and you are between 25 and 55 or plan to visit the same foreign country twice, write down your reasons.",
                Home.GetErrorMessage("ReasonForTravel"));
        }

        [Fact]
        public void select_border_return_date_and_verify_no_reason_for_long_travel_in_client_mode()
        {
            Home.SetMode("client");
            Home.WriteInput("ReturnDate", DateTime.Today.AddMonths(1).ToString("MM/dd/yyyy"));
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ReasonForLongTravel"));
        }

        [Fact]
        public void select_late_return_date_and_verify_reason_for_long_travel_in_client_mode()
        {
            Home.SetMode("client");
            Home.WriteInput("ReturnDate", DateTime.Today.AddMonths(1).AddDays(1).ToString("MM/dd/yyyy"));
            Assert.Equal(
                "If you plan to stay abroad longer than one month from now, write down your reasons.",
                Home.GetErrorMessage("ReasonForLongTravel"));
        }

        [Fact]
        public void select_late_return_date_then_clear_again_and_verify_no_reason_for_long_travel_in_client_mode()
        {
            Home.SetMode("client");
            Home.WriteInput("ReturnDate", DateTime.Today.AddMonths(1).AddDays(1).ToString("MM/dd/yyyy"));
            Home.ClearInput("ReturnDate");
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ReasonForLongTravel"));
        }

        [Fact]
        public void select_late_return_date_put_text_in_reason_for_long_travel_verify_no_reason_for_long_travel_in_client_mode()
        {
            Home.SetMode("client");
            Home.WriteInput("ReturnDate", DateTime.Today.AddMonths(1).AddDays(1).ToString("MM/dd/yyyy"));
            Home.WriteTextarea("ReasonForLongTravel", "a");
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ReasonForLongTravel"));
        }

        [Fact]
        public void select_late_return_date_put_empty_text_in_reason_for_long_travel_verify_no_reason_for_long_travel_in_client_mode()
        {
            Home.SetMode("client");
            Home.WriteInput("ReturnDate", DateTime.Today.AddMonths(1).AddDays(1).ToString("MM/dd/yyyy"));
            Home.WriteTextarea("ReasonForLongTravel", Keys.Enter);
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ReasonForLongTravel"));
        }

        [Fact]
        public void change_culture_select_border_return_date_and_verify_no_reason_for_long_travel_in_client_mode()
        {
            Home.SetMode("client");
            Home.SetLang("pl");
            Home.WriteInput("ReturnDate", DateTime.Today.AddMonths(1).ToString("yyyy-MM-dd"));
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ReasonForLongTravel"));
        }

        [Fact]
        public void change_culture_select_late_return_date_and_verify_reason_for_long_travel_in_client_mode()
        {
            Home.SetMode("client");
            Home.SetLang("pl");
            Home.WriteInput("ReturnDate", DateTime.Today.AddMonths(1).AddDays(1).ToString("yyyy-MM-dd"));
            Assert.Equal(
                "Jeśli planujesz przebywać za granicą dłużej niż miesiąc, podaj powody.",
                Home.GetErrorMessage("ReasonForLongTravel"));
        }

        [Fact]
        public void select_past_date_and_verify_return_date_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.WriteInput("ReturnDate", DateTime.Today.AddDays(-1).ToString("MM/dd/yyyy"));
            Assert.Equal(
                "We are afraid that going back to the past is not an option nowadays.",
                Home.GetErrorMessage("ReturnDate"));
        }

        [Fact]
        public void select_today_and_verify_return_date_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.WriteInput("ReturnDate", DateTime.Today.ToString("MM/dd/yyyy"));
            Assert.Equal(
                "The period of stay should be at least a week.",
                Home.GetErrorMessage("ReturnDate"));
        }

        [Fact]
        public void select_six_days_from_today_and_verify_return_date_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.WriteInput("ReturnDate", DateTime.Today.AddDays(6).ToString("MM/dd/yyyy"));
            Assert.Equal(
                "The period of stay should be at least a week.",
                Home.GetErrorMessage("ReturnDate"));
        }

        [Fact]
        public void select_a_week_from_today_and_verify_no_return_date_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.WriteInput("ReturnDate", DateTime.Today.AddDays(7).ToString("MM/dd/yyyy"));
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ReturnDate"));
        }

        [Fact]
        public void select_a_year_without_day_from_today_and_verify_no_return_date_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.WriteInput("ReturnDate", DateTime.Today.AddYears(1).AddDays(-1).ToString("MM/dd/yyyy"));
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ReturnDate"));
        }

        [Fact]
        public void select_a_year_from_today_and_verify_return_date_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.WriteInput("ReturnDate", DateTime.Today.AddYears(1).ToString("MM/dd/yyyy"));
            Assert.Equal(
                "The period of stay should be less than a year.",
                Home.GetErrorMessage("ReturnDate"));
        }

        [Fact]
        public void put_nonsense_to_date_and_verify_return_date_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.WriteInput("ReturnDate", "////");
            Home.Submit();
            Assert.Equal(Home.GetPostbacksCount(), 0); // this means no unhandled exception has been thrown, so no client logic has been aborted
            Assert.Equal(
                "The field Return date must be a date.", // 3rd party message
                Home.GetErrorMessage("ReturnDate"));
        }

        [Fact]
        public void select_uncertain_stability_verify_risks_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.Select("PoliticalStability", "Uncertain");
            Assert.Equal(
                "You are required to agree that you are aware of the risks of travel.",
                Home.GetErrorMessage("AwareOfTheRisks"));
        }

        [Fact]
        public void select_uncertain_stability_accept_risks_verify_no_risks_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.ClickCheckbox("AwareOfTheRisks");
            Home.Select("PoliticalStability", "Uncertain");
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("AwareOfTheRisks"));
        }

        [Fact]
        public void select_no_sport_verify_no_blood_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.ClickRadio("SportType", "None");
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("BloodType"));
        }

        [Fact]
        public void select_normal_sport_verify_blood_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.ClickRadio("SportType", "Normal");
            Assert.Equal(
                "Blood type is required if you do extreme sports, or if you do any type of sport and plan to go abroad.",
                Home.GetErrorMessage("BloodType"));
        }

        [Fact]
        public void write_incorrect_prefix_in_home_address_and_verify_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.WriteInput("ContactDetails_Addresses_0__Details", "street");
            Assert.Equal(
                "Address format should start from 'Street' prefix.",
                Home.GetErrorMessage("ContactDetails.Addresses[0].Details"));
        }

        [Fact]
        public void write_correct_prefix_in_home_address_and_verify_no_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.WriteInput("ContactDetails_Addresses_0__Details", "Street");
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ContactDetails.Addresses[0].Details"));
        }

        [Fact]
        public void change_culture_write_incorrect_prefix_in_home_address_and_verify_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.SetLang("pl");
            Home.WriteInput("ContactDetails_Addresses_0__Details", "ulica");
            Assert.Equal(
                "Format adresu powinien zaczynać się od prefixu 'Ulica'.",
                Home.GetErrorMessage("ContactDetails.Addresses[0].Details"));
        }

        [Fact]
        public void change_culture_prefix_in_home_address_and_verify_no_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.SetLang("pl");
            Home.WriteInput("ContactDetails_Addresses_0__Details", "Ulica");
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ContactDetails.Addresses[0].Details"));
        }

        [Fact]
        public void submit_and_verify_agree_for_contact_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.Submit();
            Assert.Equal(
                "You have to authorize us to contact you.",
                Home.GetErrorMessage("AgreeForContact"));
        }

        [Fact]
        public void select_agree_for_contact_submit_and_verify_no_agree_for_contact_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.ClickRadio("AgreeForContact", "True");
            Home.Submit();
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("AgreeForContact"));
        }

        [Fact]
        public void fill_email_home_address_agree_for_contact_verify_error_for_immediate_contact_in_client_mode()
        {
            Home.SetMode("client");
            Home.WriteInput("ContactDetails_Email", "a");
            Home.WriteInput("ContactDetails_Addresses_0__Details", "a");
            Home.ClickRadio("AgreeForContact", "True");
            Assert.Equal(
                "Shall we contact immidiately?",
                Home.GetErrorMessage("ImmediateContact"));
        }

        [Fact]
        public void fill_email_home_address_agree_for_contact_select_immidiet_contact_to_false_verify_no_error_for_immediate_contact_in_client_mode()
        {
            Home.SetMode("client");
            Home.WriteInput("ContactDetails_Email", "a");
            Home.WriteInput("ContactDetails_Addresses_0__Details", "a");
            Home.ClickRadio("AgreeForContact", "True");
            Home.Select("ImmediateContact", "No");
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("ImmediateContact"));
        }

        [Fact]
        public void submit_and_verify_donations_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.Submit();
            Assert.Equal(
                "The Donation [USD] field is required by the following logic: GoAbroad == true",
                Home.GetErrorMessage("SelectedDonations"));
        }

        [Fact]
        public void select_single_donation_and_verify_donations_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.ClickCheckbox("SelectedDonations", "1");
            Assert.Equal(
                "At least two separate donations are required.",
                Home.GetErrorMessage("SelectedDonations"));
        }

        [Fact]
        public void select_two_donations_and_verify_no_donations_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.ClickCheckbox("SelectedDonations", "1");
            Home.ClickCheckbox("SelectedDonations", "4");
            Assert.Equal(
                string.Empty,
                Home.GetErrorMessage("SelectedDonations"));
        }

        [Fact]
        public void put_letter_in_phone_verify_phone_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.WriteInput("ContactDetails_Phone", "a");
            Assert.Equal(
                "Only digits are accepted.",
                Home.GetErrorMessage("ContactDetails.Phone"));
        }

        [Fact]
        public void put_digit_in_phone_verify_phone_error_in_client_mode()
        {
            Home.SetMode("client");
            Home.WriteInput("ContactDetails_Phone", "1");
            Assert.Equal(
                "Phone number should contain from 9 to 15 digits.",
                Home.GetErrorMessage("ContactDetails.Phone"));
        }
    }
}
