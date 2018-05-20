package pageObjectModel;

import pageObjectModel.pages.HotelBookingPage;
import pageObjectModel.pages.HotelBookingPageImpl;
import org.apache.commons.lang3.StringUtils;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.chrome.ChromeDriver;
import org.openqa.selenium.chrome.ChromeOptions;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.TimeUnit;

public class WebManager {
    private WebDriver driver;

    public WebDriver getDriver() {
        return driver;
    }

    public void initialize() throws Exception {
        if (driver == null) {
            driver = getWebdriver("chrome", 10000, false, "");
            createPages();
        }
    }

    private WebDriver getWebdriver(String browserType,
                                   int implicitWaitTimeOutInMilliseconds,
                                   boolean openInPrivateMode,
                                   String driverLocation) throws Exception {
        String driverDirectoryPath = driverLocation;
        if (StringUtils.isBlank(driverDirectoryPath)) {
            driverDirectoryPath = System.getProperty("user.dir") + "/drivers/chromedriver.exe";
        }

        System.setProperty("webdriver.chrome.driver", driverDirectoryPath);

        int timeoutInMinutes = 30;

        WebDriver webdriver = null;
        switch (browserType) {
            case "chrome": {
                ChromeOptions chromeOptions = new ChromeOptions();
                List<String> arguments = new ArrayList<String>();
                arguments.add("no-sandbox");
                arguments.add("disable-infobars");
                if (openInPrivateMode) {
                    arguments.add("incognito");
                }
                chromeOptions.addArguments(arguments);

                webdriver = new ChromeDriver(chromeOptions);

                webdriver.manage().window().maximize();
            }
            break;

            default:
                throw new Exception("Testing on \"" + browserType + "\" browser is not yet supported.");
        }

        webdriver.manage().timeouts().implicitlyWait(implicitWaitTimeOutInMilliseconds, TimeUnit.MILLISECONDS);
        return webdriver;
    }

    public void cleanup() {
        driver.manage().deleteAllCookies();
        driver.quit();
    }

    public void openWebpage(String strURL) {
        driver.navigate().to(strURL);
    }

    public String getURL() {
        return driver.getCurrentUrl();
    }

    private void createPages() {
        hotelBookingPage = new HotelBookingPageImpl(driver);
    }

    private HotelBookingPage hotelBookingPage;

    public HotelBookingPage getHotelBookingPage() {
        return hotelBookingPage;
    }
}
