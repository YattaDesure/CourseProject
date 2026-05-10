import tkinter as tk
from tkinter import ttk


def main():
    root = tk.Tk()
    root.title("ЛР 2_3, вариант 8 — Задание 1")
    root.geometry("640x320")

    ttk.Label(root, text="Задание 1.\nМассив X=[1,5,1,4].\nЗаменить все элементы < 5 на 111.").pack(
        anchor="w", padx=10, pady=(10, 6)
    )

    out = tk.Text(root, height=8, wrap="word")
    out.pack(fill="both", expand=True, padx=10, pady=(6, 10))

    mode = tk.StringVar(value="show")  # radiobutton
    opts = ttk.LabelFrame(root, text="Показать")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Исходный и новый", variable=mode, value="show").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Только новый", variable=mode, value="new").pack(
        side="left", padx=8, pady=6
    )

    def run():
        x = [1, 5, 1, 4]
        y = []
        for v in x:
            if v < 5:
                y.append(111)
            else:
                y.append(v)

        out.delete("1.0", "end")
        if mode.get() == "new":
            out.insert("1.0", "Новый: " + str(y))
        else:
            out.insert("1.0", "Исходный: " + str(x) + "\nНовый: " + str(y))

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=(0, 6))

    root.mainloop()


if __name__ == "__main__":
    main()

