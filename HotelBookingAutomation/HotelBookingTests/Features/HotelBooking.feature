@HotelBooking
Feature: HotelBooking
	Testing of hotel booking features

Scenario Outline: User should be able to do booking by entering valid values in all fields
 Given I open the Hotel Booking website
 When I enter <Name> as the Firstname
 And I enter <Surname> as the Surname
 And I enter <Price> as the Price
 And I set <Deposit> as the Deposit
 And I enter <CheckInDate> as the Check-in date
 And I enter <CheckOutDate> as the Check-out date
 And I click the Save button
 Then The record should have get saved

 Examples: 
 | Name  | Surname | Price | Deposit | CheckInDate | CheckOutDate |
 | Aseem | Sharma  | 100   | true    | 2018-05-12  | 2018-05-15   |
 | Brock | Lesner  | 500   | false   | 2018-05-12  | 2018-05-15   |


Scenario: User should be able to delete a booking
 Given I open the Hotel Booking website
 When I find out a record with Aseem as Firstname
 And I click the Delete button corresponding to that record
 Then That record should get deleted
