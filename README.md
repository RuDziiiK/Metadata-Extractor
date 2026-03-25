# Metadata Extractror

## 📖 O projekcie
**Metadata Analyzer** to modułowa aplikacja napisana w języku C#, służąca do analizy struktur plików `.dll` (bibliotek) oraz `.exe` (programów wykonywalnych) platformy .NET. Aplikacja wykorzystuje mechanizm **refleksji (System.Reflection)**, aby odczytywać metadane (przestrzenie nazw, klasy, metody, właściwości) bez konieczności uruchamiania badanego kodu czy posiadania jego źródeł.

Głównym celem edukacyjnym i architektonicznym projektu jest demonstracja kompozycji aplikacji, wzorca MVVM, wstrzykiwania zależności (MEF) oraz wariantowego podejścia do interfejsu użytkownika, magazynowania danych i logowania.

## ✨ Główne funkcje
* 🔍 **Analiza metadanych:** Ekstrakcja informacji o strukturze plików `.dll`/`.exe` do specjalnego modelu obiektowego.
* 🖥️ **Wiele interfejsów (UI):** Do wyboru interfejs graficzny (WPF) lub tekstowy (CLI). Oba korzystają ze wspólnej logiki.
* 💾 **Wariantowe repozytorium:** Zapis i odczyt zbadanych metadanych do pliku **XML** lub **relacyjnej bazy danych**.
* 📝 **Śledzenie działania (Tracing):** Rejestrowanie logów aplikacji do **pliku tekstowego** lub **bazy danych**.
* 🧩 **Modułowość:** Wykorzystanie **MEF (Managed Extensibility Framework)** do dynamicznego wstrzykiwania i podmieniania implementacji poszczególnych modułów.
* ⚡ **Asynchroniczność:** Główne operacje odczytu i zapisu wykonywane są w tle, bez blokowania interfejsu użytkownika.

## 🛠️ Architektura i Technologie
* **Język:** C# / .NET
* **Architektura:** MVVM (Model-View-ViewModel) z luźnymi powiązaniami (Data Bindings).
* **Kompozycja:** MEF (Managed Extensibility Framework).
* **Testowanie:** Testy jednostkowe z użyciem techniki Mock (wstrzykiwanie zależności).
* **Interfejsy:** WPF (Windows Presentation Foundation) oraz Console Application.
