using Xunit;

namespace ExpressiveAnnotations.MvcWebSample.UITests
{
    public class ClientModeTest : BaseTest
    {
        [Fact]
        public void client_validation_switch_should_be_highlited_in_client_mode()
        {            
            Home.SetClientMode();
            Assert.Equal("[client]", Home.GetSelectedMode());
        }

        [Fact]
        public void unselect_goabroad_and_check_blood_error_in_client_mode()
        {
            Home.SetClientMode();
            Home.ClickGoAbroad();
            Assert.Equal("Blood type is required if you do extreme sports, or if you do any type of sport and plan to go abroad.", Home.GetBloodTypeError());
        }

        [Fact]
        public void change_culture_unselect_goabroad_and_check_blood_error_in_client_mode()
        {
            Home.SetClientMode();
            Home.SetPolishLang();
            Home.ClickGoAbroad();
            Assert.Equal("Typ krwi jest wymagany jeśli uprawiasz sporty ekstremalne, lub planujesz wyjechać za granicę i uprawiasz jakikolwiek sport.", Home.GetBloodTypeError());
        }

        [Fact]
        public void unselect_change_trigger_unselect_goabroad_and_check_no_blood_error_in_client_mode()
        {
            Home.SetClientMode();
            Home.ClickChangeTrigger();
            Home.ClickGoAbroad();
            Assert.Equal(string.Empty, Home.GetBloodTypeError());
        }        
    }
}
