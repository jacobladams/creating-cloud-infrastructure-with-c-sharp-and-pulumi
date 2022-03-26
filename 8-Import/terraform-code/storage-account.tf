terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 2.84"
    }
  }
  required_version = ">= 1.0.11"
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "rg" {
  name     = "rg-terraform-demo"
  location = "CentralUS"
}

resource "azurerm_storage_account" "example" {
  name                     = "stterraformdemo314"
  resource_group_name      = "rg-terraform-demo"
  location                 = "CentralUS"
  account_tier             = "Standard"
  account_replication_type = "LRS"
}