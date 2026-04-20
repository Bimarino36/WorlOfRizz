using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleRestaurant.Localization
{
    public enum GameLanguage
    {
        English = 0,
        Russian = 1
    }

    public static class LocalizationService
    {
        private const string SaveKey = "IdleRestaurant.Language";

        private static readonly Dictionary<string, string> EnglishTexts = CreateEnglishTexts();
        private static readonly Dictionary<string, string> RussianTexts = CreateRussianTexts();

        private static GameLanguage currentLanguage = LoadLanguage();

        public static event Action LanguageChanged;

        public static GameLanguage CurrentLanguage => currentLanguage;

        public static bool IsRussian => currentLanguage == GameLanguage.Russian;

        public static string CurrentLanguageCode => IsRussian ? "RU" : "ENG";

        public static void ToggleLanguage()
        {
            SetLanguage(IsRussian ? GameLanguage.English : GameLanguage.Russian);
        }

        public static void SetLanguage(GameLanguage language)
        {
            if (currentLanguage == language)
            {
                return;
            }

            currentLanguage = language;
            PlayerPrefs.SetInt(SaveKey, (int)currentLanguage);
            PlayerPrefs.Save();
            LanguageChanged?.Invoke();
        }

        public static string Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            Dictionary<string, string> dictionary = IsRussian ? RussianTexts : EnglishTexts;
            if (dictionary.TryGetValue(key, out string translated))
            {
                return translated;
            }

            if (EnglishTexts.TryGetValue(key, out translated))
            {
                return translated;
            }

            return key;
        }

        public static string Format(string key, params object[] args)
        {
            string template = Get(key);
            return args == null || args.Length == 0
                ? template
                : string.Format(template, args);
        }

        private static GameLanguage LoadLanguage()
        {
            int rawValue = PlayerPrefs.GetInt(SaveKey, (int)GameLanguage.Russian);
            return Enum.IsDefined(typeof(GameLanguage), rawValue)
                ? (GameLanguage)rawValue
                : GameLanguage.Russian;
        }

        private static Dictionary<string, string> CreateEnglishTexts()
        {
            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["common.restaurant"] = "Restaurant",
                ["common.adventure"] = "Adventure",
                ["common.farm"] = "Farm",
                ["common.claim_amount"] = "Claim ${0}",
                ["common.claim_none"] = "No Claim",
                ["common.status.complete"] = "Complete",
                ["common.status.locked"] = "Locked",
                ["common.status.ready_to_serve"] = "Ready to serve",
                ["common.status.missing"] = "Missing",
                ["common.requirements.none"] = "None",
                ["common.resource_missing"] = "Missing resources.",
                ["common.resource.rare"] = "Rare",
                ["common.resource.seeds"] = "Seeds",
                ["common.resource.ingredients"] = "Ingredients",
                ["common.resource.ingredients_short"] = "Ing",
                ["common.duration.minutes"] = "{0}m",
                ["common.duration.hours_minutes"] = "{0}h {1}m",
                ["common.offline.title"] = "Offline Income",
                ["common.offline.welcome"] = "Welcome back!",
                ["common.offline.away"] = "Away: {0}",
                ["common.offline.raw_final"] = "Raw ${0} -> Final ${1}",
                ["common.offline.cap"] = "Cap ${0} (stage {1})",
                ["common.offline.earned"] = "Earned ${0}",
                ["common.offline.claim"] = "Claim +${0}",

                ["rest.stats.cash"] = "Cash ${0}",
                ["rest.stats.guests_queue"] = "Guests {0}  Queue {1}",
                ["rest.stats.served_walkouts"] = "Served {0}  Walkouts {1}",
                ["rest.stats.queue_loyalty"] = "Queue WO {0}  Loyalty {1}",
                ["rest.stats.waiter"] = "Waiter: {0} ({1})",
                ["rest.ui.upgrades"] = "Upgrades",
                ["rest.ui.mode"] = "Mode: {0}",
                ["rest.ui.action_buy"] = "Buy $",
                ["rest.ui.action_need"] = "Need $",
                ["rest.upgrade.tables"] = "Tables Lv.{0}  {1}{2}\nIncome x{3}",
                ["rest.upgrade.waiter"] = "Waiter Lv.{0}  {1}{2}\nSpeed x{3}",
                ["rest.upgrade.kitchen"] = "Kitchen Lv.{0}  {1}{2}\nSpeed x{3}",
                ["rest.upgrade.bar"] = "Bar Lv.{0}  {1}{2}\nSpeed x{3}",
                ["rest.notify.runtime_not_ready"] = "System not ready",
                ["rest.notify.need_table_upgrade"] = "Need ${0} for table upgrade",
                ["rest.notify.need_waiter_upgrade"] = "Need ${0} for waiter upgrade",
                ["rest.notify.need_kitchen_upgrade"] = "Need ${0} for kitchen upgrade",
                ["rest.notify.need_bar_upgrade"] = "Need ${0} for bar upgrade",
                ["rest.notify.offline_income"] = "Offline income +${0}",
                ["rest.notify.waiter_mode"] = "Waiter mode: {0}",
                ["rest.notify.income_tips"] = "Income +${0}  Tips +${1}",
                ["rest.notify.guest_walked_out"] = "Guest walked out",
                ["rest.notify.queue_guest_left"] = "Queue guest left",
                ["rest.notify.tables_upgraded"] = "Tables upgraded to Lv.{0}  Income x{1}",
                ["rest.notify.waiter_upgraded"] = "Waiter upgraded to Lv.{0}  Speed x{1}",
                ["rest.notify.kitchen_upgraded"] = "Kitchen upgraded to Lv.{0}  Speed x{1}",
                ["rest.notify.bar_upgraded"] = "Bar upgraded to Lv.{0}  Speed x{1}",
                ["rest.notify.meta_reward"] = "Meta reward",
                ["rest.notify.loyalty_positive"] = "Loyalty +{0}",
                ["rest.notify.loyalty_negative"] = "Loyalty {0}",
                ["rest.notify.expedition_payout"] = "Expedition payout",
                ["rest.notify.special_order_missing"] = "Special order is missing.",
                ["rest.notify.special_order_complete"] = "{0} is already complete.",
                ["rest.notify.special_order_failed"] = "Failed to consume special-order resources.",
                ["rest.notify.pending_none"] = "No pending restaurant coins yet.",
                ["rest.portal.adventure_missing"] = "Adventure scene is not ready in build settings yet.",
                ["rest.portal.farm_missing"] = "Farm scene is not ready in build settings yet.",
                ["rest.contracts.title"] = "Restaurant Contracts",
                ["rest.contracts.intro"] = "Adventure gives Rare and Seeds. Farm turns Seeds into Ingredients.",
                ["rest.contracts.need"] = "Need: {0}",
                ["rest.contracts.reward"] = "Reward: {0}",
                ["rest.contracts.status"] = "Status: {0}",
                ["rest.contract.garden_soup.title"] = "Garden Soup",
                ["rest.contract.garden_soup.desc"] = "Use fresh farm ingredients to unlock a stronger premium table contract.",
                ["rest.contract.garden_soup.reward"] = "$90 instant cash",
                ["rest.contract.garden_soup.lock"] = "Need farm ingredients",
                ["rest.contract.signature.title"] = "Signature Feast",
                ["rest.contract.signature.desc"] = "Mix an adventure rare resource with farm ingredients for a VIP contract.",
                ["rest.contract.signature.reward"] = "$150 cash and loyalty +6",
                ["rest.contract.signature.lock"] = "Need rare resource and ingredients",
                ["rest.contract.action.done"] = "Done",
                ["rest.contract.action.serve"] = "Serve",
                ["rest.contract.action.locked"] = "Locked",
                ["rest.contract.lock_need"] = "Need {0}",
                ["rest.ops.title"] = "Operations",
                ["rest.ops.kitchen"] = "Kitchen: {0}",
                ["rest.ops.bar"] = "Bar: {0}",
                ["rest.ops.floor"] = "Floor: Clean x{0} / Bills x{1}",
                ["rest.ops.queue"] = "Queue: {0} / Loyalty: {1}",
                ["rest.ops.ready"] = "Ready x{0}",
                ["rest.ops.prep"] = "Prep x{0}",
                ["rest.ops.mix"] = "Mix x{0}",
                ["rest.ops.idle"] = "Idle",
                ["rest.actor.cook_ready"] = "Cook\nReady x{0}",
                ["rest.actor.cook_cooking"] = "Cook\nCooking x{0}",
                ["rest.actor.cook_idle"] = "Cook\nIdle",
                ["rest.actor.bar_ready"] = "Bar\nReady x{0}",
                ["rest.actor.bar_mixing"] = "Bar\nMixing x{0}",
                ["rest.actor.bar_idle"] = "Bar\nIdle",
                ["rest.actor.manager_mood_down"] = "Manager\nMood down",
                ["rest.actor.manager_clean"] = "Manager\nClean x{0}",
                ["rest.actor.manager_bills"] = "Manager\nBills x{0}",
                ["rest.actor.manager_queue"] = "Manager\nQueue x{0}",
                ["rest.actor.manager_watching"] = "Manager\nWatching",
                ["rest.station.entrance_queue"] = "Entrance\nQueue x{0}",
                ["rest.station.entrance_open"] = "Entrance\nOpen",
                ["rest.station.order_submit"] = "Order Desk\nSubmit x{0}",
                ["rest.station.order_new"] = "Order Desk\nNew x{0}",
                ["rest.station.order_idle"] = "Order Desk\nIdle",
                ["rest.station.kitchen_ready"] = "Kitchen Pickup\nReady x{0}",
                ["rest.station.kitchen_prep"] = "Kitchen Pickup\nPrep x{0}",
                ["rest.station.kitchen_idle"] = "Kitchen Pickup\nIdle",
                ["rest.station.bar_ready"] = "Bar Pickup\nReady x{0}",
                ["rest.station.bar_mix"] = "Bar Pickup\nMix x{0}",
                ["rest.station.bar_idle"] = "Bar Pickup\nIdle",
                ["rest.station.sink_clear"] = "Sink / Waste\nClear x{0}",
                ["rest.station.sink_idle"] = "Sink / Waste\nIdle",
                ["rest.station.cashier_bills"] = "Cashier\nBills x{0}",
                ["rest.station.cashier_open"] = "Cashier\nOpen",
                ["rest.table.seating"] = "Seating",
                ["rest.table.order"] = "Order",
                ["rest.table.prep"] = "Prep",
                ["rest.table.serve"] = "Serve",
                ["rest.table.eating"] = "Eating",
                ["rest.table.clean"] = "Clean",
                ["rest.table.bill"] = "Bill",
                ["rest.waiter.priority.balanced"] = "Balanced",
                ["rest.waiter.priority.speed"] = "Speed",
                ["rest.waiter.priority.tip"] = "Tip",
                ["rest.waiter.task.idle"] = "Idle",
                ["rest.waiter.task.take_order"] = "Take Order",
                ["rest.waiter.task.submit_order"] = "Submit Order",
                ["rest.waiter.task.pickup_kitchen"] = "Kitchen Pickup",
                ["rest.waiter.task.pickup_bar"] = "Bar Pickup",
                ["rest.waiter.task.deliver_order"] = "Deliver",
                ["rest.waiter.task.cleanup"] = "Cleanup",
                ["rest.waiter.task.process_bill"] = "Process Bill",

                ["adv.stats.title"] = "Adventure Slice",
                ["adv.stats.wave"] = "Wave {0}/{1}",
                ["adv.stats.enemies"] = "Enemies {0}",
                ["adv.stats.hp"] = "HP {0}/{1}",
                ["adv.stats.status"] = "Status: {0}",
                ["adv.button.use_burst"] = "Use Burst",
                ["adv.button.burst_cd"] = "Burst {0}s",
                ["adv.button.leave_run"] = "Leave Run",
                ["adv.results.waves"] = "Waves cleared: {0}",
                ["adv.results.rare"] = "Rare +{0}",
                ["adv.results.seeds"] = "Seeds +{0}",
                ["adv.results.coins"] = "Restaurant coins +${0}",
                ["adv.results.victory"] = "Victory",
                ["adv.results.defeat"] = "Defeat",
                ["adv.results.return"] = "Return to Restaurant",
                ["adv.results.retry"] = "Retry Run",
                ["adv.status.prepare"] = "Prepare",
                ["adv.status.next_wave"] = "Next wave",
                ["adv.status.skill_burst"] = "Skill burst",
                ["adv.status.wave_number"] = "Wave {0}",
                ["adv.status.boss_wave"] = "Boss wave",
                ["adv.status.run_complete"] = "Run complete",
                ["adv.status.run_failed"] = "Run failed",
                ["adv.warning.restaurant_scene_missing"] = "Restaurant scene is not available in build settings.",

                ["farm.stats.title"] = "Farm Garden",
                ["farm.stats.seeds"] = "Seeds {0}",
                ["farm.stats.ingredients"] = "Ingredients {0}",
                ["farm.stats.rare"] = "Rare {0}",
                ["farm.stats.pending"] = "Pending coins ${0}",
                ["farm.status.start"] = "Plant seeds from Adventure.",
                ["farm.plot.default"] = "Plot",
                ["farm.plot.grow"] = "Plot {0}\nGrow {1}s",
                ["farm.plot.harvest"] = "Plot {0}\nHarvest +{1}",
                ["farm.plot.plant"] = "Plot {0}\nPlant Seed",
                ["farm.status.harvested"] = "Harvested +{0} ingredients.",
                ["farm.status.not_ready"] = "Plot is not ready yet.",
                ["farm.status.planted"] = "Plot {0} planted.",
                ["farm.status.need_seeds"] = "Need seeds from Adventure.",
                ["farm.status.growing"] = "Crop is still growing.",
                ["farm.order.default"] = "Order",
                ["farm.order.reward_rare"] = "Rare +{0} | Coins +${1}",
                ["farm.order.reward_coins"] = "Coins +${0}",
                ["farm.order.label"] = "{0}\nNeed {1} ing\n{2}",
                ["farm.status.not_enough_ingredients"] = "Not enough ingredients.",
                ["farm.status.order_completed"] = "{0} completed.",
                ["farm.order.soup_bundle"] = "Soup Bundle",
                ["farm.order.rare_basket"] = "Rare Basket",
                ["farm.plot_view.base"] = "Plot {0}",
                ["farm.plot_view.empty"] = "Empty",
                ["farm.plot_view.grow"] = "Grow {0}s",
                ["farm.plot_view.ready"] = "Ready +{0}",
                ["farm.warning.restaurant_scene_missing"] = "Restaurant scene is not available in build settings."
            };
        }

        private static Dictionary<string, string> CreateRussianTexts()
        {
            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["common.restaurant"] = "Ресторан",
                ["common.adventure"] = "Приключение",
                ["common.farm"] = "Ферма",
                ["common.claim_amount"] = "Забрать ${0}",
                ["common.claim_none"] = "Нет награды",
                ["common.status.complete"] = "Выполнено",
                ["common.status.locked"] = "Закрыто",
                ["common.status.ready_to_serve"] = "Готово к подаче",
                ["common.status.missing"] = "Нет",
                ["common.requirements.none"] = "Нет",
                ["common.resource_missing"] = "Не хватает ресурсов.",
                ["common.resource.rare"] = "Редк.",
                ["common.resource.seeds"] = "Семена",
                ["common.resource.ingredients"] = "Ингредиенты",
                ["common.resource.ingredients_short"] = "Инг",
                ["common.duration.minutes"] = "{0}м",
                ["common.duration.hours_minutes"] = "{0}ч {1}м",
                ["common.offline.title"] = "Оффлайн доход",
                ["common.offline.welcome"] = "С возвращением!",
                ["common.offline.away"] = "Оффлайн: {0}",
                ["common.offline.raw_final"] = "Без лимита ${0} -> Итого ${1}",
                ["common.offline.cap"] = "Лимит ${0} (этап {1})",
                ["common.offline.earned"] = "Заработано ${0}",
                ["common.offline.claim"] = "Забрать +${0}",

                ["rest.stats.cash"] = "Касса ${0}",
                ["rest.stats.guests_queue"] = "Гости {0}  Очередь {1}",
                ["rest.stats.served_walkouts"] = "Обслужено {0}  Ушли {1}",
                ["rest.stats.queue_loyalty"] = "Оч. ушли {0}  Лояльность {1}",
                ["rest.stats.waiter"] = "Официант: {0} ({1})",
                ["rest.ui.upgrades"] = "Апгрейды",
                ["rest.ui.mode"] = "Режим: {0}",
                ["rest.ui.action_buy"] = "Купить $",
                ["rest.ui.action_need"] = "Нужно $",
                ["rest.upgrade.tables"] = "Столы ур.{0}  {1}{2}\nДоход x{3}",
                ["rest.upgrade.waiter"] = "Официант ур.{0}  {1}{2}\nСкорость x{3}",
                ["rest.upgrade.kitchen"] = "Кухня ур.{0}  {1}{2}\nСкорость x{3}",
                ["rest.upgrade.bar"] = "Бар ур.{0}  {1}{2}\nСкорость x{3}",
                ["rest.notify.runtime_not_ready"] = "Система еще не готова",
                ["rest.notify.need_table_upgrade"] = "Нужно ${0} на апгрейд столов",
                ["rest.notify.need_waiter_upgrade"] = "Нужно ${0} на апгрейд официанта",
                ["rest.notify.need_kitchen_upgrade"] = "Нужно ${0} на апгрейд кухни",
                ["rest.notify.need_bar_upgrade"] = "Нужно ${0} на апгрейд бара",
                ["rest.notify.offline_income"] = "Оффлайн доход +${0}",
                ["rest.notify.waiter_mode"] = "Режим официанта: {0}",
                ["rest.notify.income_tips"] = "Доход +${0}  Чаевые +${1}",
                ["rest.notify.guest_walked_out"] = "Гость ушел",
                ["rest.notify.queue_guest_left"] = "Гость ушел из очереди",
                ["rest.notify.tables_upgraded"] = "Столы ур.{0}  Доход x{1}",
                ["rest.notify.waiter_upgraded"] = "Официант ур.{0}  Скорость x{1}",
                ["rest.notify.kitchen_upgraded"] = "Кухня ур.{0}  Скорость x{1}",
                ["rest.notify.bar_upgraded"] = "Бар ур.{0}  Скорость x{1}",
                ["rest.notify.meta_reward"] = "Мета награда",
                ["rest.notify.loyalty_positive"] = "Лояльность +{0}",
                ["rest.notify.loyalty_negative"] = "Лояльность {0}",
                ["rest.notify.expedition_payout"] = "Награда вылазки",
                ["rest.notify.special_order_missing"] = "Контракт не найден.",
                ["rest.notify.special_order_complete"] = "{0} уже выполнен.",
                ["rest.notify.special_order_failed"] = "Не удалось списать ресурсы контракта.",
                ["rest.notify.pending_none"] = "Пока нет награды в кассу.",
                ["rest.portal.adventure_missing"] = "Сцена Adventure еще не добавлена в build settings.",
                ["rest.portal.farm_missing"] = "Сцена Farm еще не добавлена в build settings.",
                ["rest.contracts.title"] = "Контракты ресторана",
                ["rest.contracts.intro"] = "Приключение дает редкий ресурс и семена. Ферма превращает семена в ингредиенты.",
                ["rest.contracts.need"] = "Нужно: {0}",
                ["rest.contracts.reward"] = "Награда: {0}",
                ["rest.contracts.status"] = "Статус: {0}",
                ["rest.contract.garden_soup.title"] = "Садовый суп",
                ["rest.contract.garden_soup.desc"] = "Используй свежие ингредиенты с фермы, чтобы открыть усиленный контракт стола.",
                ["rest.contract.garden_soup.reward"] = "$90 сразу в кассу",
                ["rest.contract.garden_soup.lock"] = "Нужны ингредиенты с фермы",
                ["rest.contract.signature.title"] = "Фирменный пир",
                ["rest.contract.signature.desc"] = "Смешай редкий ресурс из приключения и ингредиенты с фермы для VIP-контракта.",
                ["rest.contract.signature.reward"] = "$150 в кассу и лояльность +6",
                ["rest.contract.signature.lock"] = "Нужен редкий ресурс и ингредиенты",
                ["rest.contract.action.done"] = "Готово",
                ["rest.contract.action.serve"] = "Подать",
                ["rest.contract.action.locked"] = "Закрыто",
                ["rest.contract.lock_need"] = "Нужно {0}",
                ["rest.ops.title"] = "Операции",
                ["rest.ops.kitchen"] = "Кухня: {0}",
                ["rest.ops.bar"] = "Бар: {0}",
                ["rest.ops.floor"] = "Зал: Уборка x{0} / Счета x{1}",
                ["rest.ops.queue"] = "Очередь: {0} / Лояльность: {1}",
                ["rest.ops.ready"] = "Готово x{0}",
                ["rest.ops.prep"] = "Готовится x{0}",
                ["rest.ops.mix"] = "Микс x{0}",
                ["rest.ops.idle"] = "Простой",
                ["rest.actor.cook_ready"] = "Повар\nГотово x{0}",
                ["rest.actor.cook_cooking"] = "Повар\nГотовит x{0}",
                ["rest.actor.cook_idle"] = "Повар\nСвободен",
                ["rest.actor.bar_ready"] = "Бар\nГотово x{0}",
                ["rest.actor.bar_mixing"] = "Бар\nСмешивает x{0}",
                ["rest.actor.bar_idle"] = "Бар\nСвободен",
                ["rest.actor.manager_mood_down"] = "Менеджер\nСпад настроя",
                ["rest.actor.manager_clean"] = "Менеджер\nУборка x{0}",
                ["rest.actor.manager_bills"] = "Менеджер\nСчета x{0}",
                ["rest.actor.manager_queue"] = "Менеджер\nОчередь x{0}",
                ["rest.actor.manager_watching"] = "Менеджер\nНаблюдает",
                ["rest.station.entrance_queue"] = "Вход\nОчередь x{0}",
                ["rest.station.entrance_open"] = "Вход\nОткрыт",
                ["rest.station.order_submit"] = "Стойка\nПробить x{0}",
                ["rest.station.order_new"] = "Стойка\nНовые x{0}",
                ["rest.station.order_idle"] = "Стойка\nСвободна",
                ["rest.station.kitchen_ready"] = "Выдача кухни\nГотово x{0}",
                ["rest.station.kitchen_prep"] = "Выдача кухни\nГотовится x{0}",
                ["rest.station.kitchen_idle"] = "Выдача кухни\nСвободна",
                ["rest.station.bar_ready"] = "Выдача бара\nГотово x{0}",
                ["rest.station.bar_mix"] = "Выдача бара\nМикс x{0}",
                ["rest.station.bar_idle"] = "Выдача бара\nСвободна",
                ["rest.station.sink_clear"] = "Мойка / мусор\nУборка x{0}",
                ["rest.station.sink_idle"] = "Мойка / мусор\nСвободно",
                ["rest.station.cashier_bills"] = "Касса\nСчета x{0}",
                ["rest.station.cashier_open"] = "Касса\nОткрыта",
                ["rest.table.seating"] = "Посадка",
                ["rest.table.order"] = "Заказ",
                ["rest.table.prep"] = "Готовка",
                ["rest.table.serve"] = "Подача",
                ["rest.table.eating"] = "Еда",
                ["rest.table.clean"] = "Уборка",
                ["rest.table.bill"] = "Счет",
                ["rest.waiter.priority.balanced"] = "Баланс",
                ["rest.waiter.priority.speed"] = "Скорость",
                ["rest.waiter.priority.tip"] = "Чаевые",
                ["rest.waiter.task.idle"] = "Свободен",
                ["rest.waiter.task.take_order"] = "Принять заказ",
                ["rest.waiter.task.submit_order"] = "Пробить заказ",
                ["rest.waiter.task.pickup_kitchen"] = "Забрать кухню",
                ["rest.waiter.task.pickup_bar"] = "Забрать бар",
                ["rest.waiter.task.deliver_order"] = "Подать заказ",
                ["rest.waiter.task.cleanup"] = "Уборка",
                ["rest.waiter.task.process_bill"] = "Счет",

                ["adv.stats.title"] = "Приключение",
                ["adv.stats.wave"] = "Волна {0}/{1}",
                ["adv.stats.enemies"] = "Враги {0}",
                ["adv.stats.hp"] = "HP {0}/{1}",
                ["adv.stats.status"] = "Статус: {0}",
                ["adv.button.use_burst"] = "Взрыв",
                ["adv.button.burst_cd"] = "Взрыв {0}с",
                ["adv.button.leave_run"] = "Выйти",
                ["adv.results.waves"] = "Пройдено волн: {0}",
                ["adv.results.rare"] = "Редк. +{0}",
                ["adv.results.seeds"] = "Семена +{0}",
                ["adv.results.coins"] = "В кассу +${0}",
                ["adv.results.victory"] = "Победа",
                ["adv.results.defeat"] = "Поражение",
                ["adv.results.return"] = "В ресторан",
                ["adv.results.retry"] = "Повторить",
                ["adv.status.prepare"] = "Подготовка",
                ["adv.status.next_wave"] = "Следующая волна",
                ["adv.status.skill_burst"] = "Взрыв",
                ["adv.status.wave_number"] = "Волна {0}",
                ["adv.status.boss_wave"] = "Босс",
                ["adv.status.run_complete"] = "Забег завершен",
                ["adv.status.run_failed"] = "Забег провален",
                ["adv.warning.restaurant_scene_missing"] = "Сцена ресторана не добавлена в build settings.",

                ["farm.stats.title"] = "Сад",
                ["farm.stats.seeds"] = "Семена {0}",
                ["farm.stats.ingredients"] = "Ингредиенты {0}",
                ["farm.stats.rare"] = "Редк. {0}",
                ["farm.stats.pending"] = "В кассу ${0}",
                ["farm.status.start"] = "Сажай семена из приключения.",
                ["farm.plot.default"] = "Грядка",
                ["farm.plot.grow"] = "Грядка {0}\nРост {1}с",
                ["farm.plot.harvest"] = "Грядка {0}\nСбор +{1}",
                ["farm.plot.plant"] = "Грядка {0}\nПосадить",
                ["farm.status.harvested"] = "Собрано +{0} ингредиентов.",
                ["farm.status.not_ready"] = "Грядка еще не готова.",
                ["farm.status.planted"] = "Грядка {0} посажена.",
                ["farm.status.need_seeds"] = "Нужны семена из приключения.",
                ["farm.status.growing"] = "Урожай еще растет.",
                ["farm.order.default"] = "Заказ",
                ["farm.order.reward_rare"] = "Редк. +{0} | В кассу +${1}",
                ["farm.order.reward_coins"] = "В кассу +${0}",
                ["farm.order.label"] = "{0}\nНужно {1} инг\n{2}",
                ["farm.status.not_enough_ingredients"] = "Недостаточно ингредиентов.",
                ["farm.status.order_completed"] = "{0} выполнен.",
                ["farm.order.soup_bundle"] = "Суповой набор",
                ["farm.order.rare_basket"] = "Редкая корзина",
                ["farm.plot_view.base"] = "Грядка {0}",
                ["farm.plot_view.empty"] = "Пусто",
                ["farm.plot_view.grow"] = "Рост {0}с",
                ["farm.plot_view.ready"] = "Готово +{0}",
                ["farm.warning.restaurant_scene_missing"] = "Сцена ресторана не добавлена в build settings."
            };
        }
    }
}
