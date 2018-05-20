package pageObjectModel.pages;

import java.util.List;

public interface HotelBookingPage
{
    boolean isVisible();

    void enterFirstName(String name);
    void enterSurname(String surname);
    void enterPrice(int price);
    void setDeposit(boolean deposit);
    void enterCheckInDate(String date);
    void enterCheckOutDate(String date);
    void clickSaveRecord();

    List<String> getAllNames();
    void deleteRecordByName(String name);
}
