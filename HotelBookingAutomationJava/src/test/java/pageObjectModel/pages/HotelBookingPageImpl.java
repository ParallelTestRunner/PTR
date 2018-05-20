package pageObjectModel.pages;

import org.openqa.selenium.By;
import org.openqa.selenium.JavascriptExecutor;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.Select;
import org.openqa.selenium.support.ui.WebDriverWait;

import java.util.List;
import java.util.stream.Collectors;

public class HotelBookingPageImpl implements HotelBookingPage {

    WebDriver driver;

    public HotelBookingPageImpl(WebDriver driver) {
        this.driver = driver;
    }

    private final String HeadingCss = ".jumbotron h1";

    private final String FirstNameTextboxCss = "#firstname";
    private final String SurnameTextboxCss = "#lastname";
    private final String PriceTextboxCss = "#totalprice";
    private final String DepositSelectControlCss = "#depositpaid";
    private final String CheckInDateTextboxCss = "#checkin";
    private final String CheckOutDateTextboxCss = "#checkout";
    private final String SaveButtonCss = "input[onclick=\"createBooking()\"]";

    private final String FirstnameColumnCss = "#bookings .row div:nth-child(1) p";

    private String getDeleteButtonCssByRowIndex(int rowIndex) {
        return "#bookings .row:nth-child(" + (rowIndex + 2) + ") input[onclick*=\"deleteBooking\"]";
    }

    public boolean isVisible() {
        WebElement element = driver.findElement(By.cssSelector(HeadingCss));
        return element.isDisplayed() && element.getText() == "Hotel booking form";
    }

    public void enterFirstName(String name) {
        driver.findElement(By.cssSelector(FirstNameTextboxCss)).sendKeys(name);
    }

    public void enterSurname(String surname) {
        driver.findElement(By.cssSelector(SurnameTextboxCss)).sendKeys(surname);
    }

    public void enterPrice(int price) {
        driver.findElement(By.cssSelector(PriceTextboxCss)).sendKeys(Integer.toString(price));
    }

    public void setDeposit(boolean deposit) {
        Select selectElement = new Select(driver.findElement(By.cssSelector(DepositSelectControlCss)));
        selectElement.selectByVisibleText(Boolean.toString(deposit).toLowerCase());
    }

    public void enterCheckInDate(String date) {
        driver.findElement(By.cssSelector(CheckInDateTextboxCss)).sendKeys(date);
    }

    public void enterCheckOutDate(String date) {
        driver.findElement(By.cssSelector(CheckOutDateTextboxCss)).sendKeys(date);
    }

    public void clickSaveRecord() {
        clickByCss(SaveButtonCss);
    }

    private void clickByCss(String cssSelector) {
        WebDriverWait webDriverWait = new WebDriverWait(driver, 10);
        try {
            webDriverWait.until(ExpectedConditions.elementToBeClickable(By.cssSelector(cssSelector)));
            driver.findElement(By.cssSelector(cssSelector)).click();
        } catch (Throwable t) {
            try {
                webDriverWait.until(ExpectedConditions.elementToBeClickable(By.cssSelector(cssSelector)));
                Thread.sleep(1000);
                driver.findElement(By.cssSelector(cssSelector)).click();

            } catch (Throwable t2) {
                ((JavascriptExecutor) driver).executeScript("document.querySelector('" + cssSelector + "').click();");
            }
        }
    }

    public List<String> getAllNames() {
        List<WebElement> nameElements = driver.findElements(By.cssSelector(FirstnameColumnCss));
        return nameElements.stream().map(x -> x.getText()).collect(Collectors.toList());
    }

    public void deleteRecordByName(String name) {
        int rowIndex = getAllNames().indexOf(name);
        clickByCss(getDeleteButtonCssByRowIndex(rowIndex));
    }
}
