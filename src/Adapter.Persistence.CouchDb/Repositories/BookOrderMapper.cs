﻿using System;
using System.Collections.Generic;
using Adapter.Persistence.CouchDb.Repositories.Dtos;
using Domain.Entities;

namespace Adapter.Persistence.CouchDb.Repositories
{
    internal static class BookOrderMapper
    {
        internal static BookOrder MapFrom(BookOrderDto retrieved)
        {
            List<OrderLine> orderLines = new List<OrderLine>();
            foreach (var orderLineDto in retrieved.OrderLines)
            {
                orderLines.Add(new OrderLine(
                    orderLineDto.Title, orderLineDto.Price, orderLineDto.Quantity,
                    orderLineDto.Id));
            }

            BookOrder bookOrder = new BookOrder(retrieved.Supplier, Guid.Parse(retrieved.Id),
                retrieved.State, orderLines);

            return bookOrder;
        }

        public static BookOrderDto MapTo(BookOrder bookOrder, string rev)
        {
            List<OrderLineDto> orderLineDtos = new List<OrderLineDto>();
            foreach (var orderLine in bookOrder.OrderLines)
            {
                orderLineDtos.Add(new OrderLineDto(
                    orderLine.Id, orderLine.Title, orderLine.Price, 
                    orderLine.Quantity, orderLine.Id));
            }
            BookOrderDto bookOrderDto = new BookOrderDto(
                bookOrder.Supplier, bookOrder.Id.ToString(), bookOrder.State, orderLineDtos);

            bookOrderDto.Rev = rev;
            return bookOrderDto;
        }
    }
}