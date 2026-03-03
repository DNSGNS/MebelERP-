using System;
using System.Collections.Generic;
using System.Text;

namespace MyApp1.Resources.Other
{
    public class PriceList
    {
        // --- Материал (ЛДСП, ХДФ и т.д.) ---
        public decimal Ldsp { get; set; } = 2900;

        public decimal Edging { get; set; } = 15; // Кромка

        // --- Фурнитура (Направляющие, Петли, Ручки) ---
        public decimal Hdf { get; set; } = 700;// ХДФ
        public decimal RailNoCloser { get; set; } = 200;  // Напр БЕЗ доводчика
        public decimal RailWithCloser { get; set; } = 800; // Напр С доводчиком
        public decimal HingeNoCloser { get; set; } = 30;   // Петли БЕЗ доводчика
        public decimal HingeWithCloser { get; set; } = 125; // Петли С доводчиком

        public decimal HandleBracket { get; set; } = 300;  // Ручка СКОБА
        public decimal HandleOverhead { get; set; } = 700; // Ручка НАКЛАДНАЯ
        public decimal HandleKnob { get; set; } = 150;     // Ручка КНОПКА
        public decimal ProfileGola { get; set; } = 500;    // Профиль GOLA

        public decimal Tube { get; set; } = 230;           // Труба
        public decimal GasLift { get; set; } = 80;         // Газ лифты
        public decimal Hook { get; set; } = 90;            // Крючки
        public decimal Lighting { get; set; } = 5000;      // Подсветка

        // --- Опции из SoftlyForm ---
        public decimal BasePrice { get; set; } = 1000;     // Цоколь
        public decimal DryerPrice { get; set; } = 1500;    // Сушка
        public decimal SoftPlace { get; set; } = 3000;     // Мягкое место
        public decimal Stretching { get; set; } = 3750;    // Перетяжка
        public decimal CarriageCoupler { get; set; } = 4500; // Каретная стяжка
        public decimal SoftShieldCarriage { get; set; } = 7500; // Мягкий щит Каретка
        public decimal SoftShieldBinding { get; set; } = 5000; // Мягкий щит Перетяжка

        // --- МДФ и Фасады ---
        public decimal Mdf { get; set; } = 3500;
        public decimal Film { get; set; } = 3500;          // Пленка
        public decimal Agt { get; set; } = 5000;
        public decimal Aluminum { get; set; } = 10000;

        // --- Двери Купе ---
        public decimal DoorProfile { get; set; } = 3200;   // Профиль Двери
        public decimal DoorMirror { get; set; } = 1700;    // Зеркало (для дверей)
        public decimal DoorLdsp { get; set; } = 2900;      // Дсп Двери
        public decimal Milling { get; set; } = 25;         // Фрезеровка

        // --- Стекло / Зеркало ---
        public decimal GraphiteMirror { get; set; } = 2400; // Графит зеркало
        public decimal FrostedGlass { get; set; } = 1300;   // Матовое стекло
        public decimal GraphiteGlass { get; set; } = 1800;  // Графит стекло
        public decimal Lacobel { get; set; } = 2500;        // Лакобель

        // --- Услуги ---
        public decimal Packaging { get; set; } = 100;       // Упаковка
        public decimal Delivery { get; set; } = 2500;       // Доставка
        public decimal GarbageRemoval { get; set; } = 500;  // Вывоз мусора
        public decimal Sawing { get; set; } = 200;          // Пила
        public decimal EdgingService { get; set; } = 20;    // Кромление (работа)
        public decimal Additive { get; set; } = 150;        // Присадка
        public decimal HingeAdditive { get; set; } = 10;    // Петли присадка


    }
}
