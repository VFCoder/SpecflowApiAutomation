Feature: Cart

Test Grocery Store API Products

@validateCart

Scenario: Add items to cart
	Given I have a valid access token
	And I have created a cart
	When I add <productId> and <quantity> to the cart
	Then the cart should contain those <productId> and <quantity>

	Examples: 
	| productId	| quantity |
    | 4646      | 1        |
    | 4643      | 2        |
