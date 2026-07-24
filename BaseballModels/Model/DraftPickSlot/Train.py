# train.py
import torch
from matplotlib.figure import Figure
from tqdm import tqdm
import matplotlib.pyplot as plt

import Model.DraftPickSlot.Config as Config
from Model.DraftPickSlot.DraftModel import soft_cross_entropy, WarNet
from Model.Combined.Model.Graphing import GraphLoss

_LEARNING_RATE = 0.03
_NUM_EPOCHS = 40

def train(
        model: WarNet,
        X_train: torch.Tensor, Y_train: torch.Tensor,
        X_test: torch.Tensor, Y_test: torch.Tensor,
        num_epochs : int = _NUM_EPOCHS,
        should_output: bool = True
        ) -> tuple[list[int], list[float], list[float]]:
    
    optimizer = torch.optim.Adam(model.parameters(), lr=_LEARNING_RATE)

    epoch_counter: list[int] = []
    train_loss_hist: list[float] = []
    test_loss_hist: list[float] = []

    print_every = max(1, num_epochs // 10)

    iterator = range(1, num_epochs + 1)
    pbar = None
    if not should_output:
        pbar = tqdm(iterator, desc="Training")
        iterator = pbar

    for epoch in iterator:
        # --- forward / backward / step ---
        model.train()
        optimizer.zero_grad()
        logits = model(X_train)
        loss = soft_cross_entropy(logits, Y_train)
        loss.backward()
        optimizer.step()

        # --- record losses ---
        train_loss = loss.item()
        test_loss = _compute_loss(model, X_test, Y_test)

        epoch_counter.append(epoch)
        train_loss_hist.append(train_loss)
        test_loss_hist.append(test_loss)

        # --- output ---
        if should_output and epoch % print_every == 0:
            print(f"Epoch {epoch:>{len(str(num_epochs))}}/{num_epochs}  "
                  f"Train: {train_loss:.6f}  Test: {test_loss:.6f}")

        if pbar is not None:
            pbar.set_postfix(train=f"{train_loss:.4f}",
                             test=f"{test_loss:.4f}")

    if should_output:
        GraphLoss(epoch_counter, train_loss_hist, test_loss_hist,
                  title="Training Loss")
        plt.show()

    return epoch_counter, train_loss_hist, test_loss_hist

def evaluate(
        model: WarNet,
        X: torch.Tensor, Y: torch.Tensor
        ) -> tuple[float, torch.Tensor]:
    
    model.eval()
    with torch.no_grad():
        logits = model(X)
        loss = soft_cross_entropy(logits, Y).item()
        probs = torch.softmax(logits, dim=1)
    return loss, probs

def _compute_loss(
        model: WarNet, X: torch.Tensor, Y: torch.Tensor
        ) -> float:
    
    model.eval()
    with torch.no_grad():
        return soft_cross_entropy(model(X), Y).item()