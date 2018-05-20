package steps;

import pageObjectModel.WebManager;
import com.google.common.base.Stopwatch;
import cucumber.api.java.en.And;
import cucumber.api.java.en.Given;
import cucumber.api.java.en.Then;
import cucumber.api.java.en.When;
import org.apache.http.util.Asserts;

import java.util.HashMap;
import java.util.List;
import java.util.concurrent.TimeUnit;
import java.util.function.BooleanSupplier;

public class HotelBookingSteps {

    WebManager webManager;
    HashMap<String, Object> scenarioContext;

    public HotelBookingSteps(Hooks hooks) {
        webManager = hooks.getWebManager();
        scenarioContext = hooks.getScenarioContext();
    }

    @Given("^I open the Hotel Booking website$")
    public void iOpenTheHotelBookingWebsite() throws Throwable {
        webManager.openWebpage("http://hotel-test.equalexperts.io/");
    }

    @When("^I enter (.*) as the FirstName")
    public void iEnterNameAsTheFirstname(String firstName) throws Throwable {
        webManager.getHotelBookingPage().enterFirstName(firstName);
        scenarioContext.put("FirstName", firstName);
    }

    @And("^I enter (.*) as the Surname$")
    public void iEnterSurnameAsTheSurname(String surname) throws Throwable {
        webManager.getHotelBookingPage().enterSurname(surname);
    }

    @And("^I enter (.*) as the Price$")
    public void iEnterPriceAsThePrice(int price) throws Throwable {
        webManager.getHotelBookingPage().enterPrice(price);
    }

    @And("^I set (.*) as the Deposit$")
    public void iSetDepositAsTheDeposit(boolean deposit) throws Throwable {
        webManager.getHotelBookingPage().setDeposit(deposit);
    }

    @And("^I enter (.*) as the Check-in date$")
    public void iEnterCheckInDateAsTheCheckInDate(String checkInDate) throws Throwable {
        webManager.getHotelBookingPage().enterCheckInDate(checkInDate);
    }

    @And("^I enter (.*) as the Check-out date$")
    public void iEnterCheckOutDateAsTheCheckOutDate(String checkOutDate) throws Throwable {
        webManager.getHotelBookingPage().enterCheckOutDate(checkOutDate);
    }

    @And("^I click the Save button$")
    public void iClickTheSaveButton() throws Throwable {
        List<String> allNames = webManager.getHotelBookingPage().getAllNames();
        scenarioContext.put("RecordCount", allNames.size());
        webManager.getHotelBookingPage().clickSaveRecord();
    }

    @Then("^The record should have get saved$")
    public void theRecordShouldHaveGetSaved() throws Throwable {
        int recordCountBeforeSavingCurrentBooking = (int) scenarioContext.get("RecordCount");

        // When the record get saved, the records count would get increased by 1. So waiting to let that happen
        waitUntil(() -> webManager.getHotelBookingPage().getAllNames().size() > recordCountBeforeSavingCurrentBooking,
                10,
                300);

        List<String> allNames = webManager.getHotelBookingPage().getAllNames();
        String firstName = (String) scenarioContext.get("FirstName");

        Asserts.check(allNames.contains(firstName), "First name \"" + firstName + "\" is not found in the booking table");
    }

    @When("^I find out a record with (.*) as FirstName")
    public void iFindOutARecordToBeDeleted(String firstName) throws Throwable {
        // If the record being deleted was just created, it may not have yet appeared on the webpage.
        // So waiting to let the record appear on the webpage
        waitUntil(() -> !webManager.getHotelBookingPage().getAllNames().contains(firstName), 10, 300);
        Thread.sleep(1000);
        List<String> names = webManager.getHotelBookingPage().getAllNames();
        int indexOfNameToBeDeleted = names.indexOf(firstName);

        // Verifying to ensure that page being deleted must exists first
        Asserts.check(indexOfNameToBeDeleted != -1, "No record is found with first name: \"" + firstName + "\"");
        scenarioContext.put("FirstName", firstName);
    }

    @And("^I click the Delete button corresponding to that record$")
    public void iClickTheDeleteButtonCorrespondingToThatRecord() throws Throwable {
        List<String> allNames = webManager.getHotelBookingPage().getAllNames();
        scenarioContext.put("RecordCount", allNames.size());

        String firstName = (String) scenarioContext.get("FirstName");
        webManager.getHotelBookingPage().deleteRecordByName(firstName);
    }

    @Then("^That record should get deleted$")
    public void thatRecordShouldGetDeleted() throws Throwable {
        int recordCountBeforeDeletingTheRecord = (int) scenarioContext.get("RecordCount");

        // When the record is deleted, it may take some time to disappear from the webpage.
        // So waiting to let that happen
        waitUntil(() -> webManager.getHotelBookingPage().getAllNames().size() < recordCountBeforeDeletingTheRecord,
                10,
                300);

        List<String> allNames = webManager.getHotelBookingPage().getAllNames();

        // Since multiple records can have same firstName, its wise to verify the record count only
        Asserts.check(allNames.size() == recordCountBeforeDeletingTheRecord - 1, "Records count is same before and after deletion");
    }

    private boolean waitUntil(BooleanSupplier condition, int maxWaitTimeInSeconds, long pollingIntervalInMilliSeconds) throws InterruptedException {
        long maxWaitTimeInMilliseconds = maxWaitTimeInSeconds * 1000;
        Stopwatch stopWatch = Stopwatch.createStarted();
        boolean conditionIsCheckedAtLeastOnce = false;
        do {
            if (conditionIsCheckedAtLeastOnce) {
                // This sleep is to avoid checking the condition too often as condition may need some time to get true and
                // so would be efficient to have a short interval between any two successive checks
                Thread.sleep(pollingIntervalInMilliSeconds);
            }

            if (condition.getAsBoolean()) {
                return true;
            }

            conditionIsCheckedAtLeastOnce = true;
        }
        while (stopWatch.elapsed(TimeUnit.MILLISECONDS) < maxWaitTimeInMilliseconds);

        return false;
    }
}
