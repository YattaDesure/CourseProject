import tkinter as tk
from tkinter import filedialog, messagebox, ttk


def main():
    root = tk.Tk()
    root.title("ЛР 2_4, вариант 8 — Задание 6")
    root.geometry("820x540")

    ttk.Label(
        root,
        text="Задание 6.\nВ магазине распродажа: K самых дорогих товаров со скидкой 20%.\n"
        "Найти: 1) цену самого дорогого товара, который НЕ участвует;\n"
        "       2) целую часть от суммы всех скидок.\n"
        "Файл: первая строка «N K», далее N цен по строкам.",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    file_path = tk.StringVar(value="shop.txt")
    row = ttk.Frame(root)
    row.pack(fill="x", padx=10, pady=6)
    ttk.Label(row, text="Файл:").pack(side="left")
    ttk.Entry(row, textvariable=file_path).pack(side="left", fill="x", expand=True, padx=8)
    ttk.Button(row, text="Выбрать...", command=lambda: pick()).pack(side="left")

    mode = tk.StringVar(value="both")  # radiobutton
    opts = ttk.LabelFrame(root, text="Что вывести")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Цена и сумма скидок", variable=mode, value="both").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Только цена", variable=mode, value="price").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Только сумма скидок", variable=mode, value="sum").pack(
        side="left", padx=8, pady=6
    )

    out = tk.Text(root, height=14, wrap="word")
    out.pack(fill="both", expand=True, padx=10, pady=(6, 10))

    def pick():
        p = filedialog.askopenfilename(title="Выберите файл")
        if p:
            file_path.set(p)

    def run():
        try:
            with open(file_path.get().strip(), "r", encoding="utf-8") as f:
                lines = f.readlines()
        except Exception as e:
            messagebox.showerror("Ошибка", str(e))
            return

        if len(lines) < 1:
            messagebox.showerror("Ошибка", "Файл пустой.")
            return

        try:
            head = lines[0].split()
            n = int(head[0])
            k = int(head[1])
        except Exception:
            messagebox.showerror("Ошибка", "Первая строка должна быть «N K».")
            return

        prices = []
        for line in lines[1:]:
            s = line.strip()
            if not s:
                continue
            try:
                prices.append(int(s))
            except Exception:
                messagebox.showerror("Ошибка", f"Не число: {s}")
                return

        if len(prices) != n:
            messagebox.showerror(
                "Ошибка", f"Цен должно быть {n}, нашлось {len(prices)}."
            )
            return
        if k < 0 or k > n:
            messagebox.showerror("Ошибка", "K вне диапазона.")
            return

        sorted_p = sorted(prices, reverse=True)
        sale = sorted_p[:k]
        rest = sorted_p[k:]

        if rest:
            most_no_sale = rest[0]
        else:
            most_no_sale = 0

        s_disc = 0.0
        for p in sale:
            s_disc += p * 0.20
        whole_disc = int(s_disc)

        out.delete("1.0", "end")
        if mode.get() != "sum":
            out.insert("end", f"Самый дорогой без скидки: {most_no_sale}\n")
        if mode.get() != "price":
            out.insert("end", f"Целая часть суммы скидок: {whole_disc}\n")

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=(0, 6))
    root.mainloop()


if __name__ == "__main__":
    main()
