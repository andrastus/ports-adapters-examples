﻿using System;
using System.Linq;
using Adapter.Persistence.Test;
using Core.Entities;
using Core.Ports.Persistence;
using Core.Tests.Unit.Helpers;
using Core.UseCases;
using Core.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Core.Tests.Unit
{
    public class OrderBookUseCaseTests
    {
        private readonly IBookOrderRepository _bookOrderRepository;

        public OrderBookUseCaseTests()
        {
            _bookOrderRepository = new BookOrderRepository();
        }

        [Fact]
        public void OrderingABook_WhenOrderForSupplierDoesNotExist_ShouldCreateNewOrderForSupplier()
        {
            var sut = CreateSut();

            BookRequest bookRequest = 
                a.BookRequest
                .ForSupplier("SupplierFoo");
            
            // act
            sut.Execute(bookRequest);

            // assert
            var storedOrders = _bookOrderRepository.GetBySupplier(bookRequest.Supplier).ToList();

            storedOrders.Count.Should().Be(1);

            var storedOrder = storedOrders.First();
            storedOrder.Supplier.Should().Be("SupplierFoo");
            storedOrder.State.Should().Be(BookOrderState.Pending);
            storedOrder.OrderLines.Should().Contain(
                x => x.Title == bookRequest.Title &&
                     x.Price == bookRequest.Price &&
                     x.Quantity == bookRequest.Quantity);
        }

        [Fact]
        public void OrderABook_WhenOrderForSupplierAlreadyExists_ShouldAddBooksToExistingOrder()
        {
            var sut = CreateSut();

            CreateBookOrderWithOrderLineForSupplier("SupplierBar");

            BookRequest bookRequest =
                a.BookRequest
                    .ForSupplier("SupplierBar")
                    .ForTitle("The Color of Magic");

            // act
            sut.Execute(bookRequest);

            // assert
            var storedOrders = _bookOrderRepository.GetBySupplier(bookRequest.Supplier).ToList();

            storedOrders.Count.Should().Be(1);

            var storedOrder = storedOrders.First();

            storedOrder.Supplier.Should().Be("SupplierBar");
            storedOrder.OrderLines.Count.Should().Be(2);
            storedOrder.OrderLines.Should().Contain(
                x => x.Title == bookRequest.Title &&
                     x.Price == bookRequest.Price &&
                     x.Quantity == bookRequest.Quantity);

        }

        [Fact]
        public void OrderABook_WhenOrderForDifferentSupplierExists_ShouldCreateNewOrder()
        {
            var sut = CreateSut();

            CreateBookOrderWithOrderLineForSupplier("SupplierBar");

            BookRequest bookRequest =
                a.BookRequest
                    .ForSupplier("SupplierFoo")
                    .ForTitle("The Color of Magic");

            // act
            sut.Execute(bookRequest);

            // assert
            var ordersForSupplierBar = _bookOrderRepository.GetBySupplier("SupplierBar").ToList();
            var ordersForSupplierFoo = _bookOrderRepository.GetBySupplier("SupplierFoo").ToList();

            ordersForSupplierBar.Count.Should().Be(1);
            ordersForSupplierFoo.Count.Should().Be(1);

            var firstOrderForSupplierFoo = ordersForSupplierFoo.First();

            firstOrderForSupplierFoo.Supplier.Should().Be("SupplierFoo");
            firstOrderForSupplierFoo.OrderLines.Count.Should().Be(1);
            firstOrderForSupplierFoo.OrderLines.Should().Contain(
                x => x.Title == bookRequest.Title &&
                     x.Price == bookRequest.Price &&
                     x.Quantity == bookRequest.Quantity);

        }

        private void CreateBookOrderWithOrderLineForSupplier(string supplier)
        {
            BookOrder bookOrder = new BookOrder(
                "SupplierBar", 
                Guid.NewGuid(),
                BookOrderState.Pending);

            bookOrder.AddBookRequest(
                a.BookRequest
                .ForSupplier(supplier)
                .ForTitle("The Hobbit")
                );
            _bookOrderRepository.Store(bookOrder);
        }

        private OrderBookUseCase CreateSut()
        {
            return new OrderBookUseCase(
                _bookOrderRepository);
        }
    }
}