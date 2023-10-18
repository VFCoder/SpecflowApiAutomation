Feature: Cart

Test Grocery Store API Products

@validateCart

Scenario: Add items to cart
	Given I have a valid access token
	And I have created a cart
	When I add <products> and <quantities> to the cart
	Then the cart should contain those <products> and <quantities>

	Examples: 
	| Product ID | Quantity |
    | 4646       | 1        |
    | 4643       | 2        |
