using Xunit;

namespace ExpressiveAnnotations.MvcWebSample.UITests
{
    public class ServerModeTest: BaseTest
    {
        [Fact]
        public void server_validation_switch_should_be_highlited_in_server_mode()
        {
            Home.SetMode("server");
            Assert.Equal(
                "[server]",
                Home.GetSelectedMode());
        }

        [Fact]
        public void unselect_goabroad_and_check_blood_error_in_server_mode()
        {
            Home.SetMode("server");
            Home.ClickCheckbox("GoAbroad");
            Home.Submit();
            Assert.Equal(
                "Blood type is required if you do extreme sports, or if you do any type of sport and plan to go abroad.", 
                Home.GetErrorMessage("BloodType"));
        }

        [Fact]
        public void change_culture_unselect_goabroad_and_check_blood_error_in_server_mode()
        {
            Home.SetMode("server");
            Home.SetLang("pl");
            Home.ClickCheckbox("GoAbroad");
            Home.Submit();
            Assert.Equal(
                "Typ krwi jest wymagany jeśli uprawiasz sporty ekstremalne, lub planujesz wyjechać za granicę i uprawiasz jakikolwiek sport.", 
                Home.GetErrorMessage("BloodType"));
        }

        [Fact]
        public void select_age_in_dropdown_and_verify_complex_travel_reason_in_server_mode()
        {
            Home.SetMode("server");
            Home.Select("Age", "15");
            Home.Submit();
            Assert.Equal(
                "If you are under 18 (indicated 15 in Age field, yes - Age 15), give us a reason of your travel no matter where you go (BTW. Poland... nice choice).",
                Home.GetErrorMessage("ReasonForTravel"));
        }

        [Fact]
        public void change_culture_select_age_in_dropdown_and_verify_processed_travel_reason_in_server_mode()
        {
            Home.SetMode("server");
            Home.SetLang("pl");
            Home.Select("Age", "15");
            Home.Submit();
            Assert.Equal(
                "Jeśli planujesz jakąkolwiek podróż i nie masz ukończonych 18 lat (wskazałeś w polu Wiek wartość 15, tak - Wiek 15), podaj powody (PS. Poland... dobry wybór).",
                Home.GetErrorMessage("ReasonForTravel"));
        }

        [Fact]
        public void verify_input_with_escaped_characters_validation_in_server_mode()
        {
            const string text = @"Simon's cat named ""\\""
 (Double Backslash)";
            Home.SetMode("server");
            Home.WriteTextarea("ReasonForTravel", text);
            Home.Submit();
            Assert.Equal(
                "Sorry, it is not a question about John's cat nor Simon's cat.",
                Home.GetErrorMessage("ReasonForTravel"));
        }

        [Fact]
        public void write_letter_and_verify_phone_error_in_server_mode()
        {
            Home.SetMode("server");
            Home.WriteInput("ContactDetails_Phone", "a");
            Home.Submit();
            Assert.Equal(
                "Only digits are accepted.",
                Home.GetErrorMessage("ContactDetails.Phone"));
        }

        [Fact]
        public void put_nonsense_to_date_and_verify_return_date_error_in_server_mode()
        {
            Home.SetMode("server");
            Home.WriteInput("ReturnDate", "////");
            Home.Submit();
            Assert.Equal(
                "The value '////' is not valid for Return date.", // 3rd party message
                Home.GetErrorMessage("ReturnDate"));
        }
    }
}
