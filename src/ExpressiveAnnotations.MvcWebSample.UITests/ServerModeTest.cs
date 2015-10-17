using Xunit;

namespace ExpressiveAnnotations.MvcWebSample.UITests
{
    public class ServerModeTest: BaseTest
    {
        [Fact]
        public void server_validation_switch_should_be_highlited_in_server_mode()
        {
            Home.SetServerMode();
            Assert.Equal("[server]", Home.GetSelectedMode());
        }

        [Fact]
        public void unselect_goabroad_and_check_blood_error_in_server_mode()
        {
            Home.SetServerMode();
            Home.ClickGoAbroad();
            Home.Submit();
            Assert.Equal("Blood type is required if you do extreme sports, or if you do any type of sport and plan to go abroad.", Home.GetBloodTypeError());
        }

        [Fact]
        public void change_culture_unselect_goabroad_and_check_blood_error_in_server_mode()
        {
            Home.SetServerMode();
            Home.SetPolishLang();
            Home.ClickGoAbroad();
            Home.Submit();
            Assert.Equal("Typ krwi jest wymagany jeśli uprawiasz sporty ekstremalne, lub planujesz wyjechać za granicę i uprawiasz jakikolwiek sport.", Home.GetBloodTypeError());
        }
    }
}
