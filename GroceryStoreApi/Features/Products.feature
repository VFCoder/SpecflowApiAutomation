Feature: Products

Test Grocery Store API Products

@ValidateProductList

Scenario: Verify product list search returns valid products
	Given I have a valid access token
    When I search for all products
	Then the product list should be returned with valid properties 


Scenario: Verify filtering products by category
    Given I have a valid access token
    When I search all products by category <filter>
    Then all products in the response should belong to that category

    Examples: 
    | filter        |
    | dairy         |
    | candy         |
    | fresh-produce |


Scenario: Verify filtering products by results
    Given I have a valid access token
    When I search all products with results parameter set to 3
    Then only that number of results should be returned


Scenario: Verify filtering products by availability
    Given I have a valid access token
    When I search all products filtered by <availability>
    Then only products with that availability should be returned

    Examples:
    | availability |
    | true         |
    | false        |


Scenario: Verify filtering products by category, results, and availability
    Given I have a valid access token
    When I set the category filter to <categoryFilter>
    And I set the results filter to <resultsFilter>
    And I set the availability filter to <availabilityFilter>
    And I execute the product search
    Then the filtered products should match the criteria

    Examples:
    | categoryFilter  | resultsFilter | availabilityFilter |
    | dairy           | 2             | true               |
    | candy           | 1             | true               |
    | fresh-produce   | 3             | true               |


